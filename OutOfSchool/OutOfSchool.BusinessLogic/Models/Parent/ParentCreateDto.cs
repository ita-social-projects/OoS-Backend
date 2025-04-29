using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class ParentCreateDto
{
    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender? Gender { get; set; }

    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Date of birth is required")]
    [CustomAge(MinAge = Constants.AdultAge, ErrorMessage = Constants.DayOfBirthErrorMessage)]
    public DateTime? DateOfBirth { get; set; }
}

public static class ParentCreateDtoExtensions
{
    public static OutOfSchool.Services.Models.Parent ToModel(this ParentCreateDto dto)
        => new()
        {
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
        };
}
