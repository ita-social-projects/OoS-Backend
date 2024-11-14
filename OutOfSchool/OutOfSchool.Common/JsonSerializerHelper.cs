using System;
using System.IO;
using System.Text.Json;

namespace OutOfSchool.Common;

public static class JsonSerializerHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptionsWeb = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public static TValue? Deserialize<TValue>(string json, JsonSerializerOptions options = null)
    {
        if (options is null)
        {
            return JsonSerializer.Deserialize<TValue>(json, options: JsonSerializerOptionsWeb);
        }

        return JsonSerializer.Deserialize<TValue>(json, options);
    }

    public static TValue? Deserialize<TValue>(Stream stream, JsonSerializerOptions options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (options is null)
        {
            return JsonSerializer.Deserialize<TValue>(stream, options: JsonSerializerOptionsWeb);
        }

        return JsonSerializer.Deserialize<TValue>(stream, options);
    }

    public static object? Deserialize(string json, Type type, JsonSerializerOptions options = null)
    {
        if (options is null)
        {
            return JsonSerializer.Deserialize(json, type, options: JsonSerializerOptionsWeb);
        }

        return JsonSerializer.Deserialize(json, type, options);
    }

    public static string Serialize<TValue>(TValue value, JsonSerializerOptions options = null)
    {
        if (options is null)
        {
            return JsonSerializer.Serialize(value, options: JsonSerializerOptionsWeb);
        }

        return JsonSerializer.Serialize(value, options);
    }

    public static void Serialize<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (options is null)
        {
            JsonSerializer.Serialize(writer, value, options: JsonSerializerOptionsWeb);
            return;
        }

        JsonSerializer.Serialize(writer, value, options);
    }
}