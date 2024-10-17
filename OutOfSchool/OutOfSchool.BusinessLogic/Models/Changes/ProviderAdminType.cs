using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.BusinessLogic.Models.Changes;

[JsonConverter(typeof(StringEnumConverter))]
public enum ProviderAdminType
{
    All,
    Deputies,
    Assistants, // workshop provider admins
}