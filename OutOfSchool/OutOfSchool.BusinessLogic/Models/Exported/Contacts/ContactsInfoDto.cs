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

public static class ContactsInfoDtoExtensions
{
    public static ContactsInfoDto ToInfoDto(this OutOfSchool.Services.Models.ContactInfo.Contacts contacts)
        => new()
        {
            Title = contacts.Title,
            IsDefault = contacts.IsDefault,
            Address = contacts.Address?.ToInfoDto(),
            Phones = contacts.Phones?.ToInfoDto(),
            Emails = contacts.Emails?.ToInfoDto(),
            SocialNetworks = contacts.SocialNetworks?.ToInfoDto()
        };

    public static List<ContactsInfoDto> ToInfoDto(this IEnumerable<OutOfSchool.Services.Models.ContactInfo.Contacts> contacts)
        => contacts.MapToList(ToInfoDto);
}