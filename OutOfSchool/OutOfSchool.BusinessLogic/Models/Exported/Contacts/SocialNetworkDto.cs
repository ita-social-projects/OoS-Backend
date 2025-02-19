using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Exported.Contacts;

public class SocialNetworkInfoDto
{
    [EnumDataType(typeof(SocialNetworkContactType), ErrorMessage = Constants.EnumErrorMessage)]
    public SocialNetworkContactType Type { get; set; }

    [StringLength(Constants.MaxUnifiedUrlLength, ErrorMessage = "URL cannot exceed allowed length.")]
    public string Url { get; set; } = string.Empty;
}