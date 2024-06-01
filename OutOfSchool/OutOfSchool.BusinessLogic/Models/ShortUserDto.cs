using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Gender? Gender { get; set; }

    [DataType(DataType.Date)]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [CustomAge(MinAge = Constants.AdultAge, ErrorMessage = Constants.DayOfBirthErrorMessage)]
    public DateTime? DateOfBirth { get; set; }
}