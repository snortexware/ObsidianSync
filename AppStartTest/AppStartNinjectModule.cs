using G.Sync.Entities.Interfaces;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppStartTest
{
    public class AppStartNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IStarter>().To<Stater>();
        }
    }
}
