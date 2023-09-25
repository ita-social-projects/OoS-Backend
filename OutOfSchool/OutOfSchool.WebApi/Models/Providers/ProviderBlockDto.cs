using System.ComponentModel.DataAnnotations;
using OutOfSchool.WebApi.Validators;
namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderBlockDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public bool IsBlocked { get; set; }

    [DataType(DataType.PhoneNumber)]
    [RegularExpression(
       Constants.PhoneNumberRegexModel,
       ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.UnifiedPhoneLength)]
    [RequiredIf("IsBlocked", true, ErrorMessage = "PhoneNumber is required")]
    public string BlockPhoneNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BlockReason { get; set; }
}