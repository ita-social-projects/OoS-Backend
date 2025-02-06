using System.Text.Json.Serialization;

namespace OutOfSchool.AikomApiClient.Models.Contract;

public class AikomError
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required int Code { get; set; }

    public required string Message { get; set; }
}