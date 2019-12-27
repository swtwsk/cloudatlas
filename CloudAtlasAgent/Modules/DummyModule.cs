using CloudAtlasAgent.Modules.Messages;

namespace CloudAtlasAgent.Modules
{
    public sealed class DummyModule : IModule
    {
        public bool Equals(IModule other) => other is DummyModule;

        public void HandleMessage(IMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose() {}
    }
}