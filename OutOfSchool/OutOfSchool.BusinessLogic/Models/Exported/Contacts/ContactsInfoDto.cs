using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Contacts;

public class ContactsInfoDto
{
    [Required]
    [StringLength(Constants.ContactsTitleMaxLength)]
    public string Title { get; set; }
    
    public bool IsDefault { get; set; }

    public AddressInfoDto Address { get; set; }
    
    public List<PhoneNumberInfoDto> Phones { get; set; } = [];
    
    public List<EmailInfoDto> Emails { get; set; } = [];
    
    public List<SocialNetworkInfoDto> SocialNetworks { get; set; } = [];
}