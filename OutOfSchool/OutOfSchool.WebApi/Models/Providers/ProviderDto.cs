using OutOfSchool.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderDto : ProviderBaseDto
{
    [Required]
    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; }

    [DataType(DataType.PhoneNumber)]
    [RegularExpression(
       Constants.PhoneNumberRegexModel,
       ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.UnifiedPhoneLength)]
    [Required(ErrorMessage = "PhoneNumber is required")]
    public string BlockPhoneNumber { get; set; } = string.Empty;
}