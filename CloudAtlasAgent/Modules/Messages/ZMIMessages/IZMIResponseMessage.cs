namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public interface IZMIResponseMessage<out T> : IMessage
        where T : new()
    {
        IZMIRequestMessage Request { get; }
        T Response { get; }
    }
}
