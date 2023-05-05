using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models;

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
    public DateTime? DateOfBirth { get; set; }
}