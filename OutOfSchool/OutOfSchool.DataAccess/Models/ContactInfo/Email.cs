using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.ContactInfo;

public class Email
{
    public string Type { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Address { get; set; }
}
