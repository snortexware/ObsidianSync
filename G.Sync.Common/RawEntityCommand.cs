using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Common
{
    public class RawEntityCommand
    {
        public string? CommandText { get; set; }
        public DynamicParameters Parameters { get; private set; } = new DynamicParameters();

        public RawEntityCommand() { }

        public RawEntityCommand(string commandText)
        {
            CommandText = commandText;
        }

    }
}
