using System;
using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Util.JsonTools;

public class TimespanConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    /// Format: Hours:Minutes
    /// </summary>
    public const string TimeSpanFormatString = @"hh\:mm";

    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        var timespanFormatted = $"{value.ToString(TimeSpanFormatString)}";
        writer.WriteValue(timespanFormatted);
    }

    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        TimeSpan.TryParseExact((string)reader.Value, TimeSpanFormatString, null, out var parsedTimeSpan);
        return parsedTimeSpan;
    }
}