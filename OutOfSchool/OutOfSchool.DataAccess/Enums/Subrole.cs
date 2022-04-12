using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Subrole
    {
        None,
        ProviderDeputy,
        ProviderAdmin,
    }
}
