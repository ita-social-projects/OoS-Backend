using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class Email
{
    [DataType(DataType.EmailAddress)]
    [MaxLength(Constants.MaxEmailAddressLength)]
    public string EmailAddress { get; set; }
}
