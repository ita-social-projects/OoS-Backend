using System.Text.Json;

namespace OutOfSchool.Common;

public static class JsonSerializerHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public static TValue? Deserialize<TValue>(string json, JsonSerializerOptions options = null)
    {
        if (options == null)
        {
            return JsonSerializer.Deserialize<TValue>(json, options: JsonSerializerOptions);
        }

        return JsonSerializer.Deserialize<TValue>(json, options);
    }

    public static TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options = null)
    {
        if (options == null)
        {
            return JsonSerializer.Deserialize<TValue>(ref reader, options: JsonSerializerOptions);
        }

        return JsonSerializer.Deserialize<TValue>(ref reader, options);
    }

    public static string Serialize<TValue>(TValue value, JsonSerializerOptions options = null)
    {
        if (options == null)
        {
            return JsonSerializer.Serialize(value, options: JsonSerializerOptions);
        }

        return JsonSerializer.Serialize(value, options);
    }

    public static void Serialize<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions options = null)
    {
        if (options == null)
        {
            JsonSerializer.Serialize(writer, value, options: JsonSerializerOptions);
            return;
        }

        JsonSerializer.Serialize(writer, value, options);
    }
}