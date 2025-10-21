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

        try
        {
            return TimeSpan.ParseExact(str, TimeSpanFormatString, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new JsonException($"Invalid TimeSpan format. Expected: {TimeSpanFormatString}.", ex);
        }
        catch (OverflowException ex)
        {
            throw new JsonException($"Invalid TimeSpan value. At least one of the numeric components is out of range or contains too many digits.", ex);
        }
    }
}