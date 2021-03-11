using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OutOfSchool.Services.Models
{
    public class Address
    {
        public long Id { get; set; }

        public string? Region { get; set; }

        public string? District { get; set; }

        [Required(ErrorMessage = "City is required")]
        [DataType(DataType.Text)]
        [MaxLength(15)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street is required")]
        [MaxLength(30)]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Building number is required")]
        [MaxLength(15)]
        public string BuildingNumb { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}