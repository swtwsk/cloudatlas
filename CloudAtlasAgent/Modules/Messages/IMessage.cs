using System;
using Ceras;

namespace CloudAtlasAgent.Modules.Messages
{
    public interface IMessage
    {
        Type Source { get; }
        Type Destination { get; }
    }
}
