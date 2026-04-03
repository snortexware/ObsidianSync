using G.Sync.Entities.Interfaces;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Repository
{
    public class RepositoryNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDatabaseInitializer>().To<DatabaseInitializer>();
            Bind<GSyncContext>().ToSelf().InSingletonScope();
            Bind<ISettingsRepository>().To<SettingsRepository>();
            Bind<IVaultsRepository>().To<VaultsRepository>();
            Bind<ITaskQueueRepository>().To<TaskQueueRepository>();
        }
    }
}
