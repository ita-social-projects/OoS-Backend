using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public class SocialNetworkDto
{
    [EnumDataType(typeof(SocialNetworkContactType), ErrorMessage = Constants.EnumErrorMessage)]
    public SocialNetworkContactType Type { get; set; }

    public string Url { get; set; } = string.Empty;
}