using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class PhoneNumberDto : IContentComparable<PhoneNumber>, IEquatable<PhoneNumberDto>
{
    [Required]
    [MinLength(Constants.PhoneNumberTypeMinLength, ErrorMessage = "Phone type should contain at least 3 characters")]
    [MaxLength(Constants.PhoneNumberTypeMaxLength, ErrorMessage = "Phone type cannot exceed 60 characters")]
    public string Type { get; set; } = null!;

    [Required]
    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string Number { get; set; } = null!;

    public override bool Equals(object obj)
    {
        if (obj is not PhoneNumberDto other)
        {
            return false;
        }

        return ReferenceEquals(this, other) || this.Equals(other);
    }
    
    public bool Equals(PhoneNumberDto other)
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
               Number == other.Number;
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        // We don't really care for "Non-readonly property referenced in 'GetHashCode()'"
        // As it is used for hashset uniques check before mapping to entity
        return HashCode.Combine(Type, Number);
    }

    public bool ContentEquals(PhoneNumber other)
    {
        if (other is null)
        {
            return false;
        }

        return string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase) &&
               Number == other.Number;
    }
}

public static class PhoneNumberDtoExtensions
{
    public static PhoneNumber SetToModel(this PhoneNumberDto phoneNumber, PhoneNumber model)
    {
        model.Type = phoneNumber.Type;
        model.Number = phoneNumber.Number;

        return model;
    }

    public static PhoneNumber ToModel(this PhoneNumberDto phoneNumber)
        => new()
        {
            Type = phoneNumber.Type,
            Number = phoneNumber.Number
        };

    public static List<PhoneNumber> ToModel(this IEnumerable<PhoneNumberDto> list)
        => list.MapToList(ToModel);

    public static PhoneNumberDto ToDto(this PhoneNumber phoneNumber)
        => new()
        {
            Type = phoneNumber.Type,
            Number = phoneNumber.Number
        };

    public static List<PhoneNumberDto> ToDto(this IEnumerable<PhoneNumber> list)
        => list.MapToList(ToDto);
}