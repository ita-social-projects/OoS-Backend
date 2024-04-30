using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderBlockDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public bool IsBlocked { get; set; }

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [RequiredIf("IsBlocked", true, ErrorMessage = "PhoneNumber is required")]
    public string BlockPhoneNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BlockReason { get; set; }
}