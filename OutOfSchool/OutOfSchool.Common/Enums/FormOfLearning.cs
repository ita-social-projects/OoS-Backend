using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormOfLearning
{
    Offline = 10,
    Online = 20,
    Mixed = 30,
}
