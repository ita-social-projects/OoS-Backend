using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft.AddressDraft;
public class AddressDraftDto
{
    [Required(ErrorMessage = "Street is required")]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Building number is required")]
    [MaxLength(15)]
    public string BuildingNumber { get; set; } = string.Empty;

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "CATOTTGId is required")]
    public long CATOTTGId { get; set; }
}
