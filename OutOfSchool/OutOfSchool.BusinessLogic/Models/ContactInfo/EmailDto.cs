using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public class EmailDto
{
    public string Type { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    public string Address { get; set; } = null!;
}