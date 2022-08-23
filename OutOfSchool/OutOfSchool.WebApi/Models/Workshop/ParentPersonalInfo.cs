using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Workshop;

public class ParentPersonalInfo : ShortUserDto
{
    [Required]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender? Gender { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
}