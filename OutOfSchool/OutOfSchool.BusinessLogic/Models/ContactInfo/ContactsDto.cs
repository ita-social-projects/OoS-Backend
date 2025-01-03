using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public class ContactsDto
{
    public string Title { get; set; }

    public ContactsAddressDto Address { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<PhoneNumberDto> Phones { get; set; } = [];

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<EmailDto> Emails { get; set; } = [];

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<SocialNetworkDto> SocialNetworks { get; set; } = [];
}