using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.ContactInfo;

public class ContactsAddress
{
    public string Street { get; set; }

    public string BuildingNumber { get; set; }

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
    public double Longitude { get; set; }

    public ulong GeoHash { get; set; } = default;

    [Required(ErrorMessage = "CATOTTGId is required")]
    public long CATOTTGId { get; set; }

    public virtual CATOTTG CATOTTG { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not ContactsAddress address)
        {
            return false;
        }

        return CATOTTGId == address.CATOTTGId &&
               string.Equals(Street, address.Street, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(BuildingNumber, address.BuildingNumber, StringComparison.OrdinalIgnoreCase);
    }

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
}