using G.Sync.Entities.Interfaces;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Common
{
    public class CommonNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IGSyncAppContext>().To<GSyncAppContext>();
        }
    }
}
