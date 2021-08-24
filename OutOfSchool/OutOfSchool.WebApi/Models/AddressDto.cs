using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class AddressDto
    {
        public long Id { get; set; }

        [MaxLength(30)]
        public string Region { get; set; } = string.Empty;

        [MaxLength(30)]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [DataType(DataType.Text)]
        [MaxLength(15)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street is required")]
        [MaxLength(30)]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Building number is required")]
        [MaxLength(15)]
        public string BuildingNumber { get; set; } = string.Empty;

        public double Latitude { get; set; } = default;

        public double Longitude { get; set; } = default;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is AddressDto address))
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
}