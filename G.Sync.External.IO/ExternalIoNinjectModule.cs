using G.Sync.Entities.Interfaces;
using G.Sync.External.IO.Quartz;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.External.IO
{
    public class ExternalIoNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IQueueStarter>().To<QueueStarter>();
            Bind<IFileWatcher>().To<FileWatcherEventsController>().InSingletonScope();
            Bind<IEventsHandler>().To<EventsHandler>();
        }
    }
}
