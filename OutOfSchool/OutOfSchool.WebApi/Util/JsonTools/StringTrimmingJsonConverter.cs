using System.Text.Json;
using System.Text.Json.Serialization;

namespace OutOfSchool.WebApi.Util.JsonTools;

/// <summary>
/// Custom JSON converter that trims leading and trailing whitespace from string values during deserialization.
/// </summary>
public class StringTrimmingJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.Trim();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value);
    }
}