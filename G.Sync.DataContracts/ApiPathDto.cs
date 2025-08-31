using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.DataContracts
{
    public class ApiPathDto
    {
        public string RootId { get; set; }
        public string BaseLocal { get; set; }
        public string RelPath { get; set; }
        public string ParentId { get; set; }
        public string PathPart { get; set; }

    }
}
