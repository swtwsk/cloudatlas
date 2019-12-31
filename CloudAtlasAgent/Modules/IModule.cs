using System;
using CloudAtlasAgent.Modules.Messages;

namespace CloudAtlasAgent.Modules
{
    public interface IModule : IEquatable<IModule>, IDisposable
    {
        void HandleMessage(IMessage message);
    }
}
