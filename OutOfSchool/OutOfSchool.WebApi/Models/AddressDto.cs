using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class AddressDto
    {
        public long Id { get; set; }

        public string Region { get; set; } = string.Empty;

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
            
            var address = obj as AddressDto;

            if (address == null)
            {
                return false;
            }

            return (Region == address.Region) && (District == address.District) && (City == address.City) && (Street == address.Street) && (BuildingNumber == address.BuildingNumber);
        }

#pragma warning disable CA1307 // Specify StringComparison
        public override int GetHashCode()
        {
            return Region.GetHashCode() ^ District.GetHashCode() ^ City.GetHashCode() ^ Street.GetHashCode() ^ BuildingNumber.GetHashCode();
        }
#pragma warning restore CA1307 // Specify StringComparison
    }
}