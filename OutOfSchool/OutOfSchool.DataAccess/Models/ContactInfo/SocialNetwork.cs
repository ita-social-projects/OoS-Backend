using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models.ContactInfo;

public class SocialNetwork
{
    [EnumDataType(typeof(SocialNetworkContactType), ErrorMessage = Constants.EnumErrorMessage)]
    public SocialNetworkContactType Type { get; set; }

    [StringLength(Constants.MaxUnifiedUrlLength, ErrorMessage = "URL cannot exceed allowed length.")]
    public string Url { get; set; }
}