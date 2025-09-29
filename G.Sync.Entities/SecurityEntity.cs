using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities
{
    public class SecurityEntity
    {
        public long Id { get; private set; }
        public string? Key { get; set; }
        public string? Token { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
