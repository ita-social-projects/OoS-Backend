using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public class PhoneNumberDto
{
    public string Type { get; set; } = null!;

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string Number { get; set; } = null!;
}