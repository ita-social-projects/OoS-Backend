using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Models;

public class AdminBaseDto
{
    [Required(ErrorMessage = "FirstName is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public string LastName { get; set; }

    public string MiddleName { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(
        Constants.PhoneNumberRegexViewModel,
        ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.UnifiedPhoneLength)]
    public string PhoneNumber { get; set; }
}