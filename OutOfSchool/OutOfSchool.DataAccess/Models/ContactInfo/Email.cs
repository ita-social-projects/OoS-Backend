using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.ContactInfo;

public class Email
{
    [Required(ErrorMessage = "Email type is required")]
    [StringLength(Constants.MaxEmailTypeLength, MinimumLength = Constants.MinEmailTypeLength, ErrorMessage = "Email type must be between 3 and 60 characters")]
    public string Type { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email address is required")]
    [StringLength(Constants.MaxEmailAddressLength, ErrorMessage = "Email address cannot exceed 254 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address format (e.g., name@example.com)")]
    public string Address { get; set; }
}
