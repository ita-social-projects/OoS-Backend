using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;
namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class JudgeDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidFirstNameErrorMessage)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
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
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    public bool IsChiefJudge { get; set; }

    [MaxLength(Constants.MaxJudgeDescriptionLength)]
    public string Description { get; set; } = string.Empty;

    public string CoverImageId { get; set; } = string.Empty;

    public Guid CompetetiveEventId { get; set; }
}