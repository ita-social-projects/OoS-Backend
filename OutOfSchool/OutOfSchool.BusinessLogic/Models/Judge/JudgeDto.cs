using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Judge;
public class JudgeDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidFirstNameErrorMessage)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidLastNameErrorMessage)]
    public string LastName { get; set; }

    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidMiddleNameErrorMessage)]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    public string CoverImageId { get; set; }

    public Guid CompetetiveEventId { get; set; }
}
