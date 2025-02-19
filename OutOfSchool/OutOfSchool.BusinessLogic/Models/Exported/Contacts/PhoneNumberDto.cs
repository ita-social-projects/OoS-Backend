using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Models.Exported.Contacts;

public class PhoneNumberInfoDto
{
    [StringLength(Constants.ContactsTitleMaxLength, ErrorMessage = "Phone type cannot exceed 60 characters")]
    public string Type { get; set; } = null!;

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string Number { get; set; } = null!;
}