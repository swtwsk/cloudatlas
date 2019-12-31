using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ceras;
using CloudAtlasAgent.Modules.Messages;
using Shared.Logger;
using Shared.Serializers;

namespace CloudAtlasAgent.Modules
{
    using SendQueue = BlockingCollection<CommunicationSendMessage>;
    using PacketsDictionary = SortedDictionary<Int32, byte[]>;
    
    public sealed class CommunicationModule : IModule
    {
        private readonly SendQueue _queue = new SendQueue(new ConcurrentQueue<CommunicationSendMessage>());

        private readonly Sender _sender;
        private readonly Thread _senderThread;

        private readonly Receiver _receiver;
        private readonly Thread _receiverThread;
        
        public CommunicationModule(IExecutor executor, int maxPacketSize, IPAddress receiverAddress, int receiverPort, 
            int receiverTimeout)
        {
            _sender = new Sender(_queue, maxPacketSize);
            _senderThread = new Thread(_sender.Start);

            _receiver = new Receiver(executor, maxPacketSize, receiverAddress, receiverPort, receiverTimeout);
            _receiverThread = new Thread(_receiver.Start);
            
            _senderThread.Start();
            _receiverThread.Start();
        }

        public void HandleMessage(IMessage message)
        {
            if (!(message is CommunicationSendMessage csm))
            {
                Logger.LogError($"{message} is not a CommunicationSendMessage!");
                return;
            }
            _queue.Add(csm);
        }

        public void Dispose()
        {
            Logger.LogWarning("Disposing CommunicationModule");
            _queue?.Dispose();
            
            _senderThread?.Interrupt();
            _sender.Dispose();
            
            _receiver.interrupted = true;
            _receiverThread?.Interrupt();
            _receiver.Dispose();
        }

        public bool Equals(IModule other) => other is CommunicationModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as CommunicationModule);
        public override int GetHashCode() => "Communication".GetHashCode();

        private sealed class Sender : IDisposable
        {
            private readonly SendQueue _queue;
            private readonly int _maxPacketSize;
            private int MaxSerializedSize => _maxPacketSize - 9;
            private byte[] _buffer;
            private byte[] _sendBuffer;
            private readonly CerasSerializer _serializer = CustomSerializer.Serializer;

            private uint msgId = 0;

            public Sender(SendQueue queue, int maxPacketSize)
            {
                _queue = queue;

                if (maxPacketSize <= 9)
                    throw new ArgumentException(
                        $"Size of the packet must be greater than 9 bytes, but it is equal to {maxPacketSize}");
                
                _maxPacketSize = maxPacketSize;
                _sendBuffer = new byte[_maxPacketSize];
            }
            
            public void Start()
            {
                try
                {
                    while (true)
                    {
                        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        var message = _queue.Take();
                        Logger.Log($"Took {message} out of queue");
                        SendMessage(message, socket);
                    }
                }
                catch (ObjectDisposedException) {}
                catch (OperationCanceledException) {}
                catch (ThreadInterruptedException) {}
                catch (Exception e) { Logger.LogException(e); }
            }

            private void SendMessage(CommunicationSendMessage message, Socket socket)
            {
                Int32 packetNumber = 1;
                
                var length = _serializer.Serialize(message.MessageToSend, ref _buffer);
                var currentPos = 0;

                while (currentPos < length)
                {
                    _sendBuffer[0] = (byte) (length - currentPos <= MaxSerializedSize ? 0 : 1);
                    var bytePacketNumber =
                        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(packetNumber));
                    Array.Copy(bytePacketNumber, 0, _sendBuffer, 1, 4);

                    var byteMessageNumber = BitConverter.GetBytes((UInt32) IPAddress.HostToNetworkOrder((Int32) msgId));
                    Array.Copy(byteMessageNumber, 0, _sendBuffer, 5, 4);
                    
                    Array.Copy(_buffer, currentPos, _sendBuffer, 9, MaxSerializedSize);
                    socket.SendTo(_sendBuffer, _maxPacketSize, 0, new IPEndPoint(message.Address, message.Port));
                    
                    currentPos += MaxSerializedSize;
                    packetNumber++;
                }

                msgId++;
                
                Logger.Log($"Message {message.MessageToSend} sent to {message.Address} at port {message.Port}");
            }

            public void Dispose()
            {
            }
        }

        private sealed class Receiver : IDisposable
        {
            private readonly IExecutor _executor;

            private readonly IDictionary<(IPAddress address, int port, UInt32 msgId), PacketsDictionary> _bytes =
                new Dictionary<(IPAddress address, int port, UInt32 msgId), PacketsDictionary>();

            private readonly IDictionary<
                (IPAddress address, int port, UInt32 msgId),
                (Int32 lastPacket, HashSet<Int32> received, HashSet<Int32> toReceive, DateTimeOffset removeTime)
            > _packetsInfo =
                new Dictionary<(IPAddress address, int port, UInt32 msgId), (Int32, HashSet<Int32>, HashSet<Int32>, DateTimeOffset)>();
            
            private readonly int _maxPacketSize;
            private int MaxSerializedSize => _maxPacketSize - 9;
            private readonly CerasSerializer _serializer = CustomSerializer.Serializer;
            private byte[] _buffer;

            private Socket _socket;
            private readonly IPAddress _ipAddress;
            private readonly int _port;
            private readonly int _receiveTimeout;
            
            private DateTimeOffset _nextRemoval;

            public Receiver(IExecutor executor, int maxPacketSize, IPAddress address, int port, int receiveTimeout)
            {
                if (maxPacketSize <= 9)
                    throw new ArgumentException(
                        $"Size of the packet must be greater than 9 bytes, but it is equal to {maxPacketSize}");

                _executor = executor;
                _maxPacketSize = maxPacketSize;
                _buffer = new byte[_maxPacketSize];
                _ipAddress = address;
                _port = port;
                _receiveTimeout = receiveTimeout;
                _nextRemoval = DateTimeOffset.Now.AddMilliseconds(receiveTimeout);
            }

            public bool interrupted = false;

            public void Start()
            {
                while (!interrupted)
                {
                    try
                    {
                        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        _socket = socket;
                        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                        socket.ReceiveTimeout = _receiveTimeout;
                        socket.Bind(new IPEndPoint(_ipAddress, _port));
                        Receive(socket);
                    }
                    catch (ThreadInterruptedException)
                    {
                        interrupted = true;
                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.Interrupted)
                        {
                            _socket?.Dispose();
                            interrupted = true;
                            return;
                        }

                        if (se.SocketErrorCode != SocketError.TimedOut)
                            Logger.LogException(se);

                        ClearOld();
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                    Thread.Sleep(50);
                }
            }
            
            private readonly byte[] _packetIdBytes = new byte[4];
            private readonly byte[] _msgIdBytes = new byte[4];

            private void Receive(Socket socket)
            {
                while (true)
                {
                    EndPoint remoteEnd = new IPEndPoint(IPAddress.Any, 0);
                    var flags = SocketFlags.None;

                    var bytesRec = socket.ReceiveMessageFrom(_buffer, 0, _buffer.Length, ref flags, ref remoteEnd,
                        out var packetInfo);
                    
                    Logger.Log($"GOT PACKET FROM {packetInfo}");
                    
                    Array.Copy(_buffer, 5, _msgIdBytes, 0, 4);
                    var msgId = (UInt32) IPAddress.NetworkToHostOrder((Int32) BitConverter.ToUInt32(_msgIdBytes));
                    var addressTuple = (packetInfo.Address, ((IPEndPoint) remoteEnd).Port, msgId);
                    if (!_bytes.TryGetValue(addressTuple, out var endPointDict))
                    {
                        endPointDict = new PacketsDictionary();
                        _bytes.Add(addressTuple, endPointDict);
                    }

                    var end = _buffer[0] == 0;

                    Array.Copy(_buffer, 1, _packetIdBytes, 0, 4);
                    var packetId = IPAddress.NetworkToHostOrder((Int32) BitConverter.ToUInt32(_packetIdBytes));
                    
                    if (!endPointDict.TryGetValue(packetId, out var packetBytes))
                    {
                        packetBytes = new byte[MaxSerializedSize];
                        endPointDict.Add(packetId, packetBytes);
                    }

                    Array.Copy(_buffer, 9, packetBytes, 0, bytesRec - 9);

                    if (!_packetsInfo.TryGetValue(addressTuple, out var tuple))
                    {
                        tuple = (end ? packetId : 0, new HashSet<int> {packetId}, end ? new HashSet<int>(Enumerable.Range(1, packetId)) : null, DateTimeOffset.Now.AddMilliseconds(_receiveTimeout));
                        _packetsInfo.Add(addressTuple, tuple);
                    }
                    else if (tuple.lastPacket == 0 && end)
                    {
                        tuple.received.Add(packetId);
                        tuple = (packetId, tuple.received, new HashSet<int>(Enumerable.Range(1, packetId)), tuple.removeTime);
                    }
                    else
                    {
                        tuple.received.Add(packetId);
                    }

                    if (tuple.lastPacket != 0 && tuple.toReceive.SetEquals(tuple.received))
                    {
                        PassMessageAtEnd(endPointDict, tuple.lastPacket);
                        _bytes.Remove(addressTuple);
                        _packetsInfo.Remove(addressTuple);
                    }
                    
                    if (_nextRemoval <= DateTimeOffset.Now)
                        ClearOld();
                }
            }

            private void PassMessageAtEnd(PacketsDictionary dictionary, int lastPacketId)
            {
                var packetElements = dictionary
                    .Where(pair => pair.Key <= lastPacketId)
                    .SelectMany(pair => pair.Value)
                    .ToArray();

                var message = _serializer.Deserialize<IMessage>(packetElements);
                Logger.Log($"Got {message}!");
                _executor.AddMessage(message);
            }

            private void ClearOld()
            {
                var toRemove = _packetsInfo
                    .Where(pair => pair.Value.removeTime <= DateTimeOffset.Now)
                    .Select(pair => pair.Key)
                    .ToList();
                // Logger.Log($"Going to remove {toRemove.Count} elements");
                toRemove.ForEach(tuple =>
                {
                    _packetsInfo.Remove(tuple);
                    _bytes.Remove(tuple);
                });
                _nextRemoval = DateTimeOffset.Now.AddMilliseconds(_receiveTimeout);
            }

            public void Dispose()
            {
                _socket?.Close();
            }
        }
    }
}