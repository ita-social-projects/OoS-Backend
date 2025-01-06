using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public class SocialNetworkDto : IContentComparable<SocialNetwork>
{
    [EnumDataType(typeof(SocialNetworkContactType), ErrorMessage = Constants.EnumErrorMessage)]
    public SocialNetworkContactType Type { get; set; }

    public string Url { get; set; } = string.Empty;
    public bool ContentEquals(SocialNetwork other)
    {
        return Type == other.Type &&
               string.Equals(Url, other.Url, StringComparison.OrdinalIgnoreCase);
    }
}