using System;
using System.Collections.Generic;

namespace CloudAtlasAgent.Modules
{
    public class ExecutorRegistry : IDisposable
    {
        private IDictionary<Type, Executor> _executorByModule = new Dictionary<Type, Executor>();
        private readonly ISet<Executor> _executors = new HashSet<Executor>();

        public void AddExecutor(Executor executor) => _executors.Add(executor);
        
        public void AddModule(IModule module, Executor executor) => _executorByModule.Add(module.GetType(), executor);

        public bool TryGetExecutor(Type moduleType, out Executor executor) =>
            _executorByModule.TryGetValue(moduleType, out executor);
        
        public bool ModuleExists(IModule module) => _executorByModule.ContainsKey(module.GetType());
        public void Dispose()
        {
            foreach (var e in _executors)
            {
                e.Dispose();
            }
        }
    }
}
