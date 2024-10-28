using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class ShortUserDto : BaseUserDto
{
    [DataType(DataType.EmailAddress)]
    public string UserName { get; set; }

    public string Role { get; set; }

    public bool IsRegistered { get; set; }

    public bool EmailConfirmed { get; set; }

    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Gender? Gender { get; set; }

    [DataType(DataType.Date)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [CustomAge(MinAge = Constants.AdultAge, ErrorMessage = Constants.DayOfBirthErrorMessage)]
    public DateTime? DateOfBirth { get; set; }
}