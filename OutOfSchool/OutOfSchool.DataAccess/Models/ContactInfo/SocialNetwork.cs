using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models.ContactInfo;

public class SocialNetwork
{
    public SocialNetworkContactType Type { get; set; }

    public string Url { get; set; }
}