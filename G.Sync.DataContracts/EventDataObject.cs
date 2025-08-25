using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;

namespace G.Sync.DataContracts
{
    public class EventDataObject
    {
        public DriveService Service { get; set; }
        public DriveService LocalRoot { get; set; }
        public DriveService DriveRoot { get; set; }
    }
}
