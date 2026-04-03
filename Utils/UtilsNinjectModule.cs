using G.Sync.Entities.Interfaces;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Utils
{
    public class UtilsNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ITaskNotifier>().To<NotifyHandler>().InSingletonScope();
        }
    }
}
