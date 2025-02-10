using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.ContactInfo;

public sealed class ContactsAddressDto : IContentComparable<ContactsAddress>, IEquatable<ContactsAddressDto>
{
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
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
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