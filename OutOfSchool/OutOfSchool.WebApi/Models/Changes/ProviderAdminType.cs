using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.WebApi.Models.Changes
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProviderAdminType
    {
        All,
        Deputies,
        Assistants, // workshop provider admins
    }
}
