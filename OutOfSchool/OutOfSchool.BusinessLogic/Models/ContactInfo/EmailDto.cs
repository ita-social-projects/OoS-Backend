using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class EmailDto : IContentComparable<Email>, IEquatable<EmailDto>
{
    [StringLength(Constants.MaxEmailTypeLength, ErrorMessage = "Email type cannot exceed 60 characters")]
    public string Type { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    [StringLength(Constants.MaxEmailAddressLength)]
    public string Address { get; set; } = null!;


    public override bool Equals(object obj)
    {
        if (obj is not EmailDto email)
        {
            return false;
        }

        return ReferenceEquals(this, email) || this.Equals(email);
    }

    public bool Equals(EmailDto other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase) && 
               string.Equals(Address, other.Address, StringComparison.OrdinalIgnoreCase);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        // We don't really care for "Non-readonly property referenced in 'GetHashCode()'"
        // As it is used for hashset uniques check before mapping to entity
        return HashCode.Combine(Type, Address);
    }

    public bool ContentEquals(Email other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Address, other.Address, StringComparison.OrdinalIgnoreCase);
    }
}