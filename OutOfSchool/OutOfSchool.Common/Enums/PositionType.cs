using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PositionType
{
    Employee = 10,
    DeputyDirector = 20,
    Director = 30,
}