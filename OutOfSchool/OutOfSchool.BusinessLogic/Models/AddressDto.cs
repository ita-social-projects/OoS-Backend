using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Models;

// TODO: This entity will stay until we fully move everything to unified contacts
public class AddressDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Street is required")]
    [MaxLength(60)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Building number is required")]
    [MaxLength(15)]
    public string BuildingNumber { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    [Required(ErrorMessage = "CATOTTGId is required")]
    public long CATOTTGId { get; set; }

    public AllAddressPartsDto CodeficatorAddressDto { get; set; }

    // Note: implementation taken from the OutOfSchool.Services.Models.Address
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 13;
            hash = (hash * 7) + CATOTTGId.GetHashCode();
            hash = (hash * 7) + (!ReferenceEquals(null, Street) ? Street.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
            hash = (hash * 7) + (!ReferenceEquals(null, BuildingNumber) ? BuildingNumber.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
            return hash;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is not AddressDto address)
        {
            return false;
        }

        return CATOTTGId == address.CATOTTGId &&
               string.Equals(Street, address.Street, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(BuildingNumber, address.BuildingNumber, StringComparison.OrdinalIgnoreCase);
    }
}

public static class AddressDtoExtensions
{
    public static AddressDraft ToDraft(this AddressDto address) 
        => new()
        {
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            CATOTTGId = address.CATOTTGId,
        };

    public static AddressDto ToDto(this AddressDraft address)
        => new()
        {
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            CATOTTGId = address.CATOTTGId,
        };

    public static AddressDto ToDto(this AddressES address)
        => new()
        {
            Id = address.Id,
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            Latitude = address.Point.GetLatitude() ?? default,
            Longitude = address.Point.GetLongitude() ?? default,
            CATOTTGId = address.CATOTTGId,
            CodeficatorAddressDto = address.CodeficatorAddressES?.ToAllAddressPartsDto(),
        };

    public static AddressDto ToDto(this ContactsAddress contactsAddress)
        => new()
        {
            Street = contactsAddress.Street,
            BuildingNumber = contactsAddress.BuildingNumber,
            Latitude = contactsAddress.Latitude,
            Longitude = contactsAddress.Longitude,
            CATOTTGId = contactsAddress.CATOTTGId,
            CodeficatorAddressDto = contactsAddress.CATOTTG?.ToAllAddressPartsDto()
        };

    public static List<AddressDto> ToDto(this IEnumerable<ContactsAddress> list)
        => list.MapToList(ToDto);
}