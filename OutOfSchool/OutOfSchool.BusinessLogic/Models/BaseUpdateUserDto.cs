using OutOfSchool.Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;
public class BaseUpdateUserDto
{
    public string Id { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string PhoneNumber { get; set; }
}
