using System;
using System.ComponentModel.DataAnnotations;
using H3Lib;
using H3Lib.Extensions;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class Address : IKeyedEntity<long>
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Street is required")]
    [MaxLength(60)]
    public string Street { get; set; }

    [Required(ErrorMessage = "Building number is required")]
    [MaxLength(15)]
    public string BuildingNumber { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    // parameter r means size (resolution) of hexagon
    public ulong GeoHash => Api.GeoToH3(default(GeoCoord).SetDegrees((decimal)Latitude, (decimal)Longitude), GeoMathHelper.Resolution);

    [Required(ErrorMessage = "CATOTTGId is required")]
    public long CATOTTGId { get; set; }

    public virtual CATOTTG CATOTTG { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        var address = obj as Address;

        if (address == null)
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
            int hash = 13;
            hash = (hash * 7) + CATOTTGId.GetHashCode();
            hash = (hash * 7) + (!ReferenceEquals(null, Street) ? Street.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
            hash = (hash * 7) + (!ReferenceEquals(null, BuildingNumber) ? BuildingNumber.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
            return hash;
        }
    }
}