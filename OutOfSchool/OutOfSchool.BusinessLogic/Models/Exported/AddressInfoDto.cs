using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported;

public class AddressInfoDto
{
    public long Id { get; set; }

    public string Street { get; set; } = string.Empty;

    public string BuildingNumber { get; set; } = string.Empty;

    public CodeficatorAddressInfoDto CodeficatorAddressDto { get; set; }
}
