﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums
{
    // TODO: Swagger ignores this attribute on model property in webapi layer
    // TODO: Ask Frontend if we can just make all enums as strings instead of magic numbers
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ApplicationStatus
    {
        Pending = 1,
        AcceptedForSelection,
        Approved,
        Rejected,
        Left,
    }
}
