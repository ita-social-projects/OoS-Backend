using OutOfSchool.Services.Models.ContactInfo;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Validators;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class EmailDto : IContentComparable<Email>, IEquatable<EmailDto>
{
    [Required(ErrorMessage = "Email type is required")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Contact type must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers, and symbols are allowed.")]
    [StringLength(Constants.MaxEmailTypeLength, MinimumLength = Constants.MinEmailTypeLength, ErrorMessage = "Email type must be between 3 and 60 characters")]
    public string Type { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email address is required")]
    [StringLength(Constants.MaxEmailAddressLength, ErrorMessage = "Email address cannot exceed 254 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address format (e.g., name@example.com)")]
    [RegularExpression(@"^[\u0021-\u007E]+$", ErrorMessage = "Only ASCII characters are allowed in the email address.")]
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