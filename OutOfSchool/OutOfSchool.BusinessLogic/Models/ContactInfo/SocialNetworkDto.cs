using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class SocialNetworkDto : IContentComparable<SocialNetwork>, IEquatable<SocialNetworkDto>
{
    [EnumDataType(typeof(SocialNetworkContactType), ErrorMessage = Constants.EnumErrorMessage)]
    public SocialNetworkContactType Type { get; set; }

    [StringLength(Constants.MaxUnifiedUrlLength, ErrorMessage = "URL cannot exceed allowed length.")]
    public string Url { get; set; } = string.Empty;
    
    public override bool Equals(object obj)
    {
        if (obj is not SocialNetworkDto socialNetwork)
        {
            return false;
        }

        return ReferenceEquals(this, socialNetwork) || this.Equals(socialNetwork);
    }

    public bool Equals(SocialNetworkDto other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Type == other.Type && string.Equals(Url, other.Url, StringComparison.OrdinalIgnoreCase);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "DTO properties are mutable by design")]
    public override int GetHashCode()
    {
        // We don't really care for "Non-readonly property referenced in 'GetHashCode()'"
        // As it is used for hashset uniques check before mapping to entity
        return HashCode.Combine(Type, Url?.ToUpperInvariant());
    }

    public bool ContentEquals(SocialNetwork other)
    {
        if (other is null)
        {
            return false;
        }

        return Type == other.Type &&
               string.Equals(Url, other.Url, StringComparison.OrdinalIgnoreCase);
    }
}