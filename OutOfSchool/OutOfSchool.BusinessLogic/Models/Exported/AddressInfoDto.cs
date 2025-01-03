namespace OutOfSchool.BusinessLogic.Models.Exported;

public class AddressInfoDto
{
    public string Street { get; set; } = string.Empty;

    public string BuildingNumber { get; set; } = string.Empty;

    public CodeficatorAddressInfoDto CodeficatorAddress { get; set; }
}
