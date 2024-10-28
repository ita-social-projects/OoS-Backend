using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Changes;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProviderAdminType
{
    All,
    Deputies,
    Assistants, // workshop provider admins
}