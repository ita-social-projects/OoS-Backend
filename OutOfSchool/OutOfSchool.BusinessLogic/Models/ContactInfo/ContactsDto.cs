using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class ContactsDto : IContentComparable<Contacts>, IEquatable<ContactsDto>
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(Constants.ContactsTitleMaxLength)]
    public string Title { get; set; }
    
    public bool IsDefault { get; set; }

    public ContactsAddressDto Address { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<PhoneNumberDto> Phones { get; set; } = [];

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<EmailDto> Emails { get; set; } = [];

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<SocialNetworkDto> SocialNetworks { get; set; } = [];

    public override bool Equals(object obj)
    {
        if (obj is not ContactsDto contacts)
        {
            return false;
        }

        return ReferenceEquals(this, contacts) || this.Equals(contacts);
    }
    
    public bool Equals(ContactsDto other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Title == other.Title 
               && IsDefault == other.IsDefault 
               && Equals(Address, other.Address);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        // We don't really care for "Non-readonly property referenced in 'GetHashCode()'"
        // As it is used for hashset uniques check before mapping to entity
        return HashCode.Combine(Title, IsDefault, Address);
    }

    public bool ContentEquals(Contacts other)
    {
        if (other is null)
        {
            return false;
        }

        // Here we don't care about nested arrays and IsDefault because it's handled
        return Title == other.Title && Address.ContentEquals(other.Address);
    }
}