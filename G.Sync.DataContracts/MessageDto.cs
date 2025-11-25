using Newtonsoft.Json;

namespace G.Sync.DataContracts
{
    public record MessageDto(int HandlerType, long TaskId)
    {
        [JsonProperty("handlerType")]
        public int HandlerType { get; private set; } = HandlerType;
        [JsonProperty("taskId")]
        public long TaskId { get; private set; } = TaskId;

        public MessageDto() : this(default, default) { }

        public MessageDto(string rawText) : this(default, default)
        {
            Console.WriteLine("Raw Text Received: " + rawText);

            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageDto>(rawText)
                ?? throw new InvalidOperationException("Deserialization resulted in null MessageDto");

            HandlerType = obj.HandlerType;
            TaskId = obj.TaskId;
        }
    }
}