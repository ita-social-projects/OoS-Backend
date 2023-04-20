using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Codeficator;

public class NearestCodeficatorRequest
{
    [Required]
    [Range(-180.0, 180.0)]
    public double Lat { get; set; }

    [Required]
    [Range(-180.0, 180.0)]
    public double Lon { get; set; }
}