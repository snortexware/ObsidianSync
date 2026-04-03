using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace G.Sync.IoC
{
    public class BusinessComponent
    {
        private static IKernel _kernel;

        public static void InitializeModules(params NinjectModule[] modules)
        {
            _kernel = new StandardKernel(modules);
        }

        public static T CreateInstance<T>()
        {
            if (_kernel == null)
                throw new InvalidOperationException("IoC not initialized");

            return _kernel.Get<T>();
        }
    }
}
