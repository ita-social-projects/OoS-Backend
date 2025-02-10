using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.Codeficator;

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
            int hash = 13;
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