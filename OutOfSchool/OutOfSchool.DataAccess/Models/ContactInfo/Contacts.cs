using System.Collections.Generic;

namespace OutOfSchool.Services.Models.ContactInfo;

public class Contacts
{
    public string Title { get; set; }
    
    public bool IsDefault { get; set; }
    
    public ContactsAddress Address { get; set; }
    public List<PhoneNumber> Phones { get; set; } = [];
    public List<Email> Emails { get; set; } = [];
    public List<SocialNetwork> SocialNetworks { get; set; } = [];
}
