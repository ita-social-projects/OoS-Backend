using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Models;

public class ProviderImportDto
{
    public string Address { get; set; }

    public string Settlement { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string Phone { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public string Identifier { get; set; }

    public string LicenseNumber { get; set; }

    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; } = OwnershipType.State;

    public string ProviderName { get; set; }
}
