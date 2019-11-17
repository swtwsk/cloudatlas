using System.Net;
using System.Net.Sockets;
using Ceras;
using Ceras.Formatters;

namespace Shared.Serializers
{
    public class IPAddressFormatter : IFormatter<IPAddress>
    {
        public void Serialize(ref byte[] buffer, ref int offset, IPAddress value)
        {
            var isIPv4 = value.AddressFamily == AddressFamily.InterNetwork;
            SerializerBinary.WriteByte(ref buffer, ref offset, (byte)(isIPv4 ? 0 : 1));
            var ip = value.GetAddressBytes();
            for (var i = 0; i < ip.Length; i++)
                SerializerBinary.WriteByte(ref buffer, ref offset, ip[i]);
        }

        public void Deserialize(byte[] buffer, ref int offset, ref IPAddress value)
        {
            var isIPv4 = SerializerBinary.ReadByte(buffer, ref offset) == 0;
            var bytes = new byte[isIPv4 ? 4 : 16];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = SerializerBinary.ReadByte(buffer, ref offset);
            value = new IPAddress(bytes);
        }
    }
}