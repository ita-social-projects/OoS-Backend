﻿using System.ComponentModel.DataAnnotations;
using OutOfSchool.WebApi.Models.Codeficator;

namespace OutOfSchool.WebApi.Models;

public class AddressDto
{
    public long Id { get; set; }

    [MaxLength(30)]
    public string Region { get; set; } = string.Empty;

    [MaxLength(30)]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    [DataType(DataType.Text)]
    [MaxLength(30)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Street is required")]
    [MaxLength(30)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Building number is required")]
    [MaxLength(15)]
    public string BuildingNumber { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public long? CATOTTGId { get; set; }

    public AllAddressPartsDto CodeficatorAddressDto { get; set; }

    // Note: implementation taken from the OutOfSchool.Services.Models.Address
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 13;
            hash = (hash * 7) + (!ReferenceEquals(null, Region) ? Region.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
            hash = (hash * 7) + (!ReferenceEquals(null, District) ? District.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
            hash = (hash * 7) + (!ReferenceEquals(null, City) ? City.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0);
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

        return string.Equals(Region, address.Region, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(District, address.District, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(City, address.City, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Street, address.Street, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(BuildingNumber, address.BuildingNumber, StringComparison.OrdinalIgnoreCase);
    }
}