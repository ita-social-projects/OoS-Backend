using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.Services.Models.ContactInfo;

public class PhoneNumber
{
    [Required]
    [MinLength(Constants.PhoneNumberTypeMinLength, ErrorMessage = "Phone type should contain at least 3 characters")]
    [MaxLength(Constants.PhoneNumberTypeMaxLength, ErrorMessage = "Phone type cannot exceed 60 characters")]
    public string Type { get; set; }

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string Number { get; set; }
}
