using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace G.Sync.Utils
{
    public class RespondeDto
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }
        [JsonPropertyName("taskId")]
        public long TaskId { get; set; }
        [JsonPropertyName("data")]
        public object Data { get; set; }
    }
}
