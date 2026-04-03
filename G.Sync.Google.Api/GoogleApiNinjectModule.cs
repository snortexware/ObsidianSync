using G.Sync.Entities.Interfaces;
using G.Sync.Google.Api.Interfaces;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Google.Api
{
    public class GoogleApiNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IGoogleDriveContext>().To<GoogleDriveContext>().InSingletonScope();
            Bind<IGoogleDriveService>().To<GoogleDriveServiceAdapter>().InSingletonScope();
            Bind<IFolderFileProcess>().To<FolderFileProcess>().InSingletonScope();
        }
    }
}
