using G.Sync.VersionSystem.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.VersionSystem.Default
{
    public class DefaultSecurityTable : IVersion
    {
        public DateTime Date => new(2025, 08, 23);

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
