using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities
{
    public partial class TaskEntity()
    {
        public enum TaskStatus
        {
            Peding = 1,
            Completed = 2,
            Failed = 3,
        }
    }
}
