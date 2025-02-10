using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.Services.Models.ContactInfo;

public class PhoneNumber
{
    public string Type { get; set; }

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string Number { get; set; }
}
