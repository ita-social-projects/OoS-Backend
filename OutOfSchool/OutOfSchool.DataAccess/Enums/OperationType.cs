﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum OperationType
{
    Create,
    Delete,
    Block,
    Update,
    Reinvite,
}