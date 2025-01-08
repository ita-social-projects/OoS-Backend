using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public class PhoneNumberDto : IContentComparable<PhoneNumber>, IEquatable<PhoneNumberDto>
{
    public string Type { get; set; } = null!;

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

        return Type == other.Type && Number == other.Number;
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
        return Type == other.Type && Number == other.Number;
    }
}