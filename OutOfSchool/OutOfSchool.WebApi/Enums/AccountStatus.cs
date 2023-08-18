using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.WebApi.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum AccountStatus
{
    NeverLogged = 0,
    Accepted = 10,
    Blocked = 20,
}