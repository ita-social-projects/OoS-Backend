using System.ComponentModel.DataAnnotations;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models.Admins;

public class BaseAdminDto
{
    public string UserId { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(
        Constants.PhoneNumberRegexModel,
        ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public string LastName { get; set; }

    public string MiddleName { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    public string FirstName { get; set; }

    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }
}