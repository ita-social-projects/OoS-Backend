using System.Text.Json.Serialization;

namespace OutOfSchool.AuthCommon.Models;

public class IdGovErrorResponse
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public int Error { get; set; } = default;

    public string? Message { get; set; }

    [JsonPropertyName("error_description")]
    public string? Description { get; set; }
}