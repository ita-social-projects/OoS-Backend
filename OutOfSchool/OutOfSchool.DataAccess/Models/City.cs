using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class City
    {
        public long Id { get; set; }

        public string Region { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "City Name is required")]
        public string Name { get; set; } = string.Empty;

        public double Latitude { get; set; } = default;

        public double Longitude { get; set; } = default;
    }
}
