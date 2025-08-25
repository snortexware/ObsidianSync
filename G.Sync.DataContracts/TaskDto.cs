using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.DataContracts
{
    /// <summary>
    /// Data Transfer Object for Task information.
    /// </summary>
    public class TaskDto
    {
        public string Name { get; set; }
        public string ExecuteTs { get; set; }
        public string FileId { get; set; }
        public int TaskType { get; set; }
        public int Status { get; set; }
    }

    public enum TaskType
    {
        Upload = 0,
        Download = 1,
        Delete = 2,
        Move = 3
    }

    public enum StatusType
    {
        pending = 1,
        completed = 2,
        failed = 3
    }
}
