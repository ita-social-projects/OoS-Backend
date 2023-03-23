using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models.SocialGroup;

namespace OutOfSchool.WebApi.Models;

public class ChildBaseDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(60, MinimumLength = 1)]
    [RegularExpression(@"^(?i)[А-ЯҐЄІЇ](([\'\-][А-ЯҐЄІЇ])?[А-ЯҐЄІЇ]*)*$", ErrorMessage = "First name contains invalid characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(60, MinimumLength = 1)]
    [RegularExpression(@"^(?i)[А-ЯҐЄІЇ](([\'\-][А-ЯҐЄІЇ])?[А-ЯҐЄІЇ]*)*$", ErrorMessage = "Last name contains invalid characters")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(60, MinimumLength = 1)]
    [RegularExpression(@"^(?i)[А-ЯҐЄІЇ](([\'\-][А-ЯҐЄІЇ])?[А-ЯҐЄІЇ]*)*$", ErrorMessage = "Middle name contains invalid characters")]
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