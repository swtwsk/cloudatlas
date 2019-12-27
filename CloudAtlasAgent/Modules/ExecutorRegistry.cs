using System;
using System.Collections.Generic;

namespace CloudAtlasAgent.Modules
{
    public class ExecutorRegistry : IDisposable
    {
        private IDictionary<IModule, Executor> _executorByModule = new Dictionary<IModule, Executor>();
        private readonly ISet<Executor> _executors = new HashSet<Executor>();

        public void AddExecutor(Executor executor) => _executors.Add(executor);
        
        public void AddModule(IModule module, Executor executor) => _executorByModule.Add(module, executor);

        public bool TryGetExecutor(IModule module, out Executor executor) =>
            _executorByModule.TryGetValue(module, out executor);
        
        public bool ModuleExists(IModule module) => _executorByModule.ContainsKey(module);
        public void Dispose()
        {
            foreach (var e in _executors)
            {
                e.Dispose();
            }
        }
    }
}
