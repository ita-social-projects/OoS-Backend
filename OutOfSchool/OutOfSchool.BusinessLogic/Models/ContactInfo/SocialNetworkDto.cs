using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class SocialNetworkDto : IContentComparable<SocialNetwork>, IEquatable<SocialNetworkDto>, IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        bool requiresUrl =
            Type == SocialNetworkContactType.Instagram ||
            Type == SocialNetworkContactType.Facebook ||
            Type == SocialNetworkContactType.Website;

        if (!requiresUrl)
            yield break;

        if (string.IsNullOrWhiteSpace(Url))
        {
            yield return new ValidationResult(
                "Url is required when type is Instagram, Facebook or Website.",
                new[] { nameof(Url) });
            yield break;
        }
        if (!Regex.IsMatch(Url, Constants.SocialNetworkUrlRegex))
        {
            yield return new ValidationResult(
                "Url must follow the format: https://example.com/username",
                new[] { nameof(Url) });
        }
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

public static class SocialNetworkDtoExtensions
{
    public static SocialNetwork SetToModel(this SocialNetworkDto socialNetwork, SocialNetwork model)
    {
        model.Type = socialNetwork.Type;
        model.Url = socialNetwork.Url;

        return model;
    }

    public static SocialNetwork ToModel(this SocialNetworkDto socialNetwork)
        => new()
        {
            Type = socialNetwork.Type,
            Url = socialNetwork.Url
        };

    public static List<SocialNetwork> ToModel(this IEnumerable<SocialNetworkDto> list)
        => list.MapToList(ToModel);

    public static SocialNetworkDto ToDto(this SocialNetwork socialNetwork)
        => new()
        {
            Type = socialNetwork.Type,
            Url = socialNetwork.Url
        };

    public static List<SocialNetworkDto> ToDto(this IEnumerable<SocialNetwork> list)
        => list.MapToList(ToDto);
}