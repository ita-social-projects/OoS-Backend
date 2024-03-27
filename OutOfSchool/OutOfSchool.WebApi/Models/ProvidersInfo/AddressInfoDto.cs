using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ProvidersInfo;

public class AddressInfoDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Street is required")]
    [MaxLength(60)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Building number is required")]
    [MaxLength(15)]
    public string BuildingNumber { get; set; } = string.Empty;

    public CodeficatorAddressInfoDto CodeficatorAddressDto { get; set; }
}
