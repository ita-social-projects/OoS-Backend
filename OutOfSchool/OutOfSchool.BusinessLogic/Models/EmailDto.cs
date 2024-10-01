using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

public class EmailDto
{
    [DataType(DataType.EmailAddress)]
    [MaxLength(Constants.MaxEmailAddressLength)]
    public string EmailAddress { get; set; }
}
