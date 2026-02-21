using System.Text.Json;
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Util.JsonTools;

public class TimespanConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    /// Format: Hours:Minutes
    /// </summary>
    public const string TimeSpanFormatString = @"hh\:mm";

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        var timespanFormatted = $"{value.ToString(TimeSpanFormatString)}";
        writer.WriteStringValue(timespanFormatted);
    }

    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException("TimeSpan value cannot be empty.");
        }

        TimeSpan.TryParseExact(reader.GetString(), TimeSpanFormatString, null, out var parsedTimeSpan);
        return parsedTimeSpan;
    }
}