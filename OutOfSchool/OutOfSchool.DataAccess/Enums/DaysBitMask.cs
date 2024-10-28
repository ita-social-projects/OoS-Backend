using System;
using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
[Flags]
public enum DaysBitMask : byte
{
    None = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 4,
    Thursday = 8,
    Friday = 16,
    Saturday = 32,
    Sunday = 64,
}