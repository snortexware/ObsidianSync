using G.Sync.Entities.Interfaces;
using G.Sync.Service.MessageFactory;
using G.Sync.Service.MessageFactory.Strategy;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service
{
    public class ServiceNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IHandler>().To<VaultsHandler>();
            Bind<IMessageHandlerFactory>().To<MessageHandlerFactory>();
            Bind<ITaskQueueService>().To<TaskQueueService>();
            Bind<IGSyncAppContextProxy>().To<GSyncAppContextProxy>().InSingletonScope();
        }
    }
}
