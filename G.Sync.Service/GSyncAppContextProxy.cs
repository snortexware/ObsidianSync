using G.Sync.Common;
using G.Sync.Entities.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service
{
    public class GSyncAppContextProxy : IGSyncAppContextProxy
    {
        [Inject]
        public IGSyncAppContext GSyncAppContext { get; set; }
        public ISettingsEntity GetSettings()
        {
            try
            {
                return GSyncAppContext.GetAppSettings();
            }
            catch
            {
                throw;
            }
        }
    }
}
