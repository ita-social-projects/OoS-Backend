using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Models;

public class PhoneNumberDto
{
    public Guid Id { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.MaxPhoneNumberLengthWithPlusSign)]
    public string Number { get; set; }
}