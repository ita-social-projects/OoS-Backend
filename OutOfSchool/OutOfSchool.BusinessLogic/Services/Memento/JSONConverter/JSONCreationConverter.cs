using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OutOfSchool.BusinessLogic.Services.Memento.JSONConverter;

public abstract class JsonCreationConverter<T> : JsonConverter
{
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);

        return typeof(T).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(serializer);

        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        JObject jObject = JObject.Load(reader);
        T target = Create(objectType, jObject);
        serializer.Populate(jObject.CreateReader(), target);

        return target;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        => throw new NotImplementedException();

    protected abstract T Create(Type objectType, JObject jObject);
}