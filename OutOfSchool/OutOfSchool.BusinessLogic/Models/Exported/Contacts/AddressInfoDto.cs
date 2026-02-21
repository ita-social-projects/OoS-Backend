using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.Exported.Contacts;

public class AddressInfoDto
{
    public string Street { get; set; } = string.Empty;

    public string BuildingNumber { get; set; } = string.Empty;

    public CodeficatorAddressInfoDto CodeficatorAddress { get; set; }
}

public static class AddressInfoDtoExtensions
{
    public static AddressInfoDto ToInfoDto(this ContactsAddress contactsAddress)
        => new()
        {
            Street = contactsAddress.Street,
            BuildingNumber = contactsAddress.BuildingNumber,
            CodeficatorAddress = contactsAddress.CATOTTG?.ToInfoDto()
        };

    public static List<AddressInfoDto> ToInfoDto(this IEnumerable<ContactsAddress> list)
        => list.MapToList(ToInfoDto);
}
