using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Address
    {
        public long Id { get; set; }

        [MaxLength(30)]
        public string Region { get; set; }

        [MaxLength(30)]
        public string District { get; set; }

        [Required(ErrorMessage = "City is required")]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        public string City { get; set; }

        [Required(ErrorMessage = "Street is required")]
        [MaxLength(30)]
        public string Street { get; set; }

        [Required(ErrorMessage = "Building number is required")]
        [MaxLength(15)]
        public string BuildingNumber { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public ulong GeoHash { get; set; } = default;

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

            return string.Equals(Region, address.Region, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(District, address.District, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(City, address.City, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Street, address.Street, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(BuildingNumber, address.BuildingNumber, StringComparison.OrdinalIgnoreCase);
        }

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
    }
}