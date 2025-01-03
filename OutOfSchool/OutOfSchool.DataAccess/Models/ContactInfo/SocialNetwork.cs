using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models.ContactInfo;

public class SocialNetwork
{
    public SocialNetworkContactType Type { get; set; }

    public string Url { get; set; } = string.Empty;
}