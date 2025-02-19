using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Contacts;

public class EmailInfoDto
{
    [StringLength(Constants.MaxEmailTypeLength, ErrorMessage = "Email type cannot exceed 60 characters")]
    public string Type { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    [StringLength(Constants.MaxEmailAddressLength)]
    public string Address { get; set; } = null!;
}