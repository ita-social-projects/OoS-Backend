using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Services.Models.ContactInfo;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class ContactsDto : IContentComparable<Contacts>, IEquatable<ContactsDto>
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(Constants.ContactsTitleMinLength)]
    [MaxLength(Constants.ContactsTitleMaxLength)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string Title { get; set; }
    
    public bool IsDefault { get; set; }

    public ContactsAddressDto Address { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<PhoneNumberDto> Phones { get; set; } = [];

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<EmailDto> Emails { get; set; } = [];

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<SocialNetworkDto> SocialNetworks { get; set; } = [];

    public override string ToString()
    {
        var phones = Phones is { Count: > 0 }
            ? string.Join(", ", Phones.Select(p => $"{p.Type}: {p.Number}"))
            : "No phones";

        var emails = Emails is { Count: > 0 }
            ? string.Join(", ", Emails.Select(e => $"{e.Type}: {e.Address}"))
            : "No emails";

        var socials = SocialNetworks is { Count: > 0 }
            ? string.Join(", ", SocialNetworks.Select(s => $"{s.Type}: {s.Url}"))
            : "No social networks";

        var address = Address != null
            ? $"Street: {Address.Street}, Building: {Address.BuildingNumber}, " +
              $"Lat: {Address.Latitude}, Long: {Address.Longitude}, CATOTTGId: {Address.CATOTTGId}"
            : "No address";

        return $"Title: {Title}, IsDefault: {IsDefault}, Address: [{address}], " +
               $"Phones: [{phones}], Emails: [{emails}], SocialNetworks: [{socials}]";
    }


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

public static class ContactsDtoExtensions
{
    public static Contacts ToModel(this ContactsDto contacts)
        => new()
        {
            Title = contacts.Title,
            IsDefault = contacts.IsDefault,
            Address = contacts.Address?.ToModel(),
            Phones = contacts.Phones?.ToModel(),
            Emails = contacts.Emails?.ToModel(),
            SocialNetworks = contacts.SocialNetworks?.ToModel()
        };

    public static List<Contacts> ToModel(this IEnumerable<ContactsDto> contacts)
        => contacts.MapToList(ToModel);

    public static ContactsDto ToDto(this Contacts contacts)
        => new()
        {
            Title = contacts.Title,
            IsDefault = contacts.IsDefault,
            Address = contacts.Address?.ToContactsDto(),
            Phones = contacts.Phones?.ToDto(),
            Emails = contacts.Emails?.ToDto(),
            SocialNetworks = contacts.SocialNetworks?.ToDto()
        };

    public static List<ContactsDto> ToDto(this IEnumerable<Contacts> contacts)
        => contacts.MapToList(ToDto);
}