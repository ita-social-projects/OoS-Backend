using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Config;

public class GeocodingConfig
{
    public const string Name = "GeoCoding";

    [Required]
    public string BaseUrl { get; set; }

    public string ApiKey { get; set; }

    [Required]
    [Range(20, 100)]
    public int Radius { get; set; }
}