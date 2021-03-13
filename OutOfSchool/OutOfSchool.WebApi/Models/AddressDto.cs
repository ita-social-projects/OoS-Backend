using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class AddressDTO
    {
        public long Id { get; set; }

        public string Region { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [DataType(DataType.Text)]
        [MaxLength(15)]
        public string City { get; set; }

        [Required(ErrorMessage = "Street is required")]
        [MaxLength(30)]
        public string Street { get; set; }

        [Required(ErrorMessage = "Building number is required")]
        [MaxLength(15)]
        public string Building { get; set; }

        public double Latitude { get; set; } = default;

        public double Longitude { get; set; } = default;
    }
}