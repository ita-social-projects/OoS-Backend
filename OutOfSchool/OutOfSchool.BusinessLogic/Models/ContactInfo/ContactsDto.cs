using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public class ContactsDto : IContentComparable<Contacts>
{
    public string Title { get; set; }
    
    public bool IsDefault { get; set; }

    public ContactsAddressDto Address { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<PhoneNumberDto> Phones { get; set; } = [];

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<EmailDto> Emails { get; set; } = [];

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<SocialNetworkDto> SocialNetworks { get; set; } = [];

    public bool ContentEquals(Contacts other)
    {
        return Title == other.Title && Address.ContentEquals(other.Address);
    }
}