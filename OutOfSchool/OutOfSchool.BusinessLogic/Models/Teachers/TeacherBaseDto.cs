using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Teachers;

/// <summary>
/// As teacher logic will be completely re-written later - property re-use and inheritance is ok here.
/// </summary>
public abstract class TeacherBaseDto
{
    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidFirstNameErrorMessage)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = Constants.RequiredLastNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidLastNameErrorMessage)]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidMiddleNameErrorMessage)]
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; } = default;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;
}