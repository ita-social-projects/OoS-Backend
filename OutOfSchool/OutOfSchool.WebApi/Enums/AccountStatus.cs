using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.WebApi.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccountStatus
    {
        NeverLogged,
        Accepted,
        Blocked,
    }
}
