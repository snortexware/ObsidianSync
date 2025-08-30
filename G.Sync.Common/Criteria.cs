using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Common
{
    public class Criteria
    {
        public DynamicParameters Parameters { get; private set; } = new DynamicParameters();
        public string? Where { get; set; }

        public Criteria() { }

        public Criteria(string where)
        {
            Where = where;
        }
    }
}
