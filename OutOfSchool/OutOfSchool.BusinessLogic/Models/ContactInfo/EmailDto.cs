using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class EmailDto : IContentComparable<Email>, IEquatable<EmailDto>
{
    [Required(ErrorMessage = "Email type is required")]
    [StringLength(Constants.MaxEmailTypeLength, MinimumLength = 3,ErrorMessage = "Email type must be between 3 and 60 characters")]
    public string Type { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email address is required")]
    [StringLength(Constants.MaxEmailAddressLength,ErrorMessage = "Email address cannot exceed 256 characters")]
    [RegularExpression(Constants.EmailRegexViewModel,ErrorMessage = "Email must contain '@' and a domain (e.g., name@example.com)")]
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

public static class EmailDtoExtensions
{
    public static Email SetToModel(this EmailDto email, Email model)
    {
        model.Type = email.Type;
        model.Address = email.Address;

        return model;
    }

    public static Email ToModel(this EmailDto email)
        => new()
        {
            Type = email.Type,
            Address = email.Address
        };

    public static List<Email> ToModel(this IEnumerable<EmailDto> list)
        => list.MapToList(ToModel);

    public static EmailDto ToDto(this Email email)
        => new()
        {
            Type = email.Type,
            Address = email.Address
        };

    public static List<EmailDto> ToDto(this IEnumerable<Email> list)
        => list.MapToList(ToDto);
}