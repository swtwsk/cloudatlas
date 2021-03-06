﻿using CloudAtlasAgent.Modules.Messages;

namespace CloudAtlasAgent.Modules
{
    public sealed class DummyModule : IModule
    {
        public bool Equals(IModule other) => other is DummyModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as DummyModule);
        public override int GetHashCode() => "Dummy".GetHashCode();

        public void HandleMessage(IMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose() {}
    }
}