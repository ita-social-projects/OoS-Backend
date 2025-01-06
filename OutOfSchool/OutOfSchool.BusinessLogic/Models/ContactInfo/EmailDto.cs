using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public class EmailDto : IContentComparable<Email>
{
    public string Type { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    public string Address { get; set; } = null!;

    public bool ContentEquals(Email other)
    {
        return Type == other.Type &&
               string.Equals(Address, other.Address, StringComparison.OrdinalIgnoreCase);
    }
}