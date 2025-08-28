using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities
{
    public partial class Task : 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExecuteTs { get; set; }
        public int Status { get; set; }
        public string FileId { get; set; }
        public int TaskType { get; set; }
    }
}
