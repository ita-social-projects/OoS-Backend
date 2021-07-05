using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class City
    {
        public long Id { get; set; }

        [MaxLength(30)]
        public string Region { get; set; } = string.Empty;

        [MaxLength(30)]
        public string District { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        public double? Latitude { get; set; } = default;

        public double? Longitude { get; set; } = default;
    }
}
