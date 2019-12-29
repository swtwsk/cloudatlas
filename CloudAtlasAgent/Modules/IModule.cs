using System;
using CloudAtlasAgent.Modules.Messages;

namespace CloudAtlasAgent.Modules
{
    public interface IModule : IEquatable<IModule>, IDisposable
    {
        IModule VoidInstance { get; }
        void HandleMessage(IMessage message);
    }
}
