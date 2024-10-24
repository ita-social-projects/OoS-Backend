using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class ChildBaseDto
{
    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
    [StringLength(Constants.NameMaxLength, MinimumLength = 1)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidFirstNameErrorMessage)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = Constants.RequiredLastNameErrorMessage)]
    [StringLength(Constants.NameMaxLength, MinimumLength = 1)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidLastNameErrorMessage)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidMiddleNameErrorMessage)]
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender? Gender { get; set; }

    [MaxLength(500)]
    public string PlaceOfStudy { get; set; } = string.Empty;

    public Guid ParentId { get; set; } = default;

    public bool IsParent { get; set; }
}