using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PositionType
{
    Director = 10,
    DeputyDirector = 20,
    Employee = 30
}