using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class CityDto
    {
        public long Id { get; set; }

        [MaxLength(30)]
        public string Region { get; set; } = string.Empty;

        [MaxLength(30)]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "City Name is required")]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        public double Latitude { get; set; } = default;

        public double Longitude { get; set; } = default;
    }
}
