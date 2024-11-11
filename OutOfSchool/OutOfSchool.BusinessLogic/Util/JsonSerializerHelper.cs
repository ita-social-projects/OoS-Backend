using System.Text.Json;

namespace OutOfSchool.BusinessLogic.Util;

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

    public static string Serialize<TValue>(TValue value, JsonSerializerOptions options = null)
    {
        if (options == null)
        {
            return JsonSerializer.Serialize(value, options: JsonSerializerOptions);
        }

        return JsonSerializer.Serialize(value, options);
    }
}