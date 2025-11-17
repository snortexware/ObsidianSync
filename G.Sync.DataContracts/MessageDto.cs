using Newtonsoft.Json;

public class MessageDto
{
    [JsonProperty("handlerType")]
    public int HandlerType { get; private set; }

    [JsonProperty("message")]
    public string Message { get; private set; }

    // Necessário para System.Text.Json NÃO quebrar
    public MessageDto() { }

    public MessageDto(string rawText)
    {
        Console.WriteLine("Raw Text Received: " + rawText);

        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageDto>(rawText)
            ?? throw new InvalidOperationException("Deserialization resulted in null MessageDto");

        HandlerType = obj.HandlerType;
        Message = obj.Message;
    }
}
