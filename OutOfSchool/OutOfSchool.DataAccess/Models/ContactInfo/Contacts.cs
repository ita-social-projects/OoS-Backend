using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.ContactInfo;

public class Contacts
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(Constants.ContactsTitleMinLength)]
    [MaxLength(Constants.ContactsTitleMaxLength)]
    public string Title { get; set; }
    
    public bool IsDefault { get; set; }
    
    public ContactsAddress Address { get; set; }
    public List<PhoneNumber> Phones { get; set; } = [];
    public List<Email> Emails { get; set; } = [];
    public List<SocialNetwork> SocialNetworks { get; set; } = [];
}
