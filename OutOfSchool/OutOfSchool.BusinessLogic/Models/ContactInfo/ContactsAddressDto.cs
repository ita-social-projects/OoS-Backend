using Elastic.Clients.Elasticsearch;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Services.Models.ContactInfo;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class ContactsAddressDto : IContentComparable<ContactsAddress>, IEquatable<ContactsAddressDto>
{
    [Required(ErrorMessage = "Street is required")]
    [MinLength(1)]
    [MaxLength(60)]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Street must contain at least one Cyrillic letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}0-9\s\-'’\.]+$", ErrorMessage = "Only Cyrillic letters, numbers, spaces, '-', '.', and apostrophe are allowed.")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Building number is required")]
    [MinLength(1)]
    [MaxLength(15)]
    [MustContain(RequiredCharacterType.Digit, ErrorMessage = "Building number must contain at least one number.")]
    [RegularExpression(@"^[0-9]+[0-9А-Яа-яЇїІіЄєҐґA-Za-z\-\/]*$", ErrorMessage = "Building number must start with a digit and may only contain Cyrillic or Latin letters, digits, '-' and '/', without spaces.")]
    public string BuildingNumber { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    [Required(ErrorMessage = "CATOTTGId is required")]
    public long CATOTTGId { get; set; }

    public AllAddressPartsDto CodeficatorAddress { get; set; }

    // Note: implementation taken from the OutOfSchool.Services.Models.Address
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 13;
            hash = (hash * 7) + CATOTTGId.GetHashCode();
            hash = (hash * 7) + (!ReferenceEquals(null, Street)
                ? Street.GetHashCode(StringComparison.OrdinalIgnoreCase)
                : 0);
            hash = (hash * 7) + (!ReferenceEquals(null, BuildingNumber)
                ? BuildingNumber.GetHashCode(StringComparison.OrdinalIgnoreCase)
                : 0);
            return hash;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is not ContactsAddressDto address)
        {
            return false;
        }

        return ReferenceEquals(this, address) || this.Equals(address);
    }

    public bool Equals(ContactsAddressDto other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return CATOTTGId == other.CATOTTGId &&
               string.Equals(Street, other.Street, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(BuildingNumber, other.BuildingNumber, StringComparison.OrdinalIgnoreCase);
    }

    public bool ContentEquals(ContactsAddress other)
    {
        if (other is null)
        {
            return false;
        }

        return CATOTTGId == other.CATOTTGId &&
               string.Equals(Street, other.Street, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(BuildingNumber, other.BuildingNumber,
                   StringComparison.OrdinalIgnoreCase);
    }
}

public static class ContactsAddressDtoExtensions
{
    public static AddressES ToES(this ContactsAddressDto contactsAddress)
        => new()
        {
            // Id - ignored in original AM mapper
            City = contactsAddress.CodeficatorAddress?.Settlement,
            Latitude = contactsAddress.Latitude,
            Longitude = contactsAddress.Longitude,
            CATOTTGId = contactsAddress.CATOTTGId,
            CodeficatorAddressES = contactsAddress.CodeficatorAddress?.ToCodeficatorAddressES(),
            Street = contactsAddress.Street,
            BuildingNumber = contactsAddress.BuildingNumber,
            Point = GeoLocation.LatitudeLongitude(new LatLonGeoLocation()
                {
                    Lat = contactsAddress.Latitude,
                    Lon = contactsAddress.Longitude,
                }),
        };

    public static ContactsAddress SetToModel(this ContactsAddressDto contactsAddress, ContactsAddress model)
    {
        model.Street = contactsAddress.Street;
        model.BuildingNumber = contactsAddress.BuildingNumber;
        model.Latitude = contactsAddress.Latitude;
        model.Longitude = contactsAddress.Longitude;
        model.CATOTTGId = contactsAddress.CATOTTGId;
        
        return model;
    }

    public static ContactsAddress ToModel(this ContactsAddressDto contactsAddress)
        => new()
        {
            Street = contactsAddress.Street,
            BuildingNumber = contactsAddress.BuildingNumber,
            Latitude = contactsAddress.Latitude,
            Longitude = contactsAddress.Longitude,
            CATOTTGId = contactsAddress.CATOTTGId,
        };

    public static List<ContactsAddress> ToModel(this IEnumerable<ContactsAddressDto> list)
        => list.MapToList(ToModel);

    public static ContactsAddressDto ToContactsDto(this ContactsAddress contactsAddress)
        => new()
        {
            Street = contactsAddress.Street,
            BuildingNumber = contactsAddress.BuildingNumber,
            Latitude = contactsAddress.Latitude,
            Longitude = contactsAddress.Longitude,
            CATOTTGId = contactsAddress.CATOTTGId,
            CodeficatorAddress = contactsAddress.CATOTTG?.ToAllAddressPartsDto()
        };

    public static List<ContactsAddressDto> ToContactsDto(this IEnumerable<ContactsAddress> list)
        => list.MapToList(ToContactsDto);
}