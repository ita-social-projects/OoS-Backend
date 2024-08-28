using System.Collections.Generic;

namespace OutOfSchool.Services.Models;

/// <summary>
/// This is just a helper class, not a real DB entity.
/// </summary>
public class OptionalContacts
{
    public List<SocialNetworkContact> SocialNetworkContacts { get; set; }

    public List<Email> Emails { get; set; }
}
