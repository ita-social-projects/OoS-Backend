namespace OutOfSchool.BusinessLogic.Models.Exported.Contacts;

public class CodeficatorAddressInfoDto
{
    public string Region { get; set; }

    public string District { get; set; }

    public string TerritorialCommunity { get; set; }

    public string Settlement { get; set; }

    public string CityDistrict { get; set; }
    
    public string Code { get; set; }
}

public static class CodeficatorAddressInfoDtoExtensions
{
    public static CodeficatorAddressInfoDto ToInfoDto(this OutOfSchool.Services.Models.CATOTTG catottg)
        => new()
        {
            Region = catottg.GetRegionName(),
            District = catottg.GetDistrictName(),
            TerritorialCommunity = catottg.GetTerritorialCommunityName(),
            Settlement = catottg.GetSettlementName(),
            CityDistrict = catottg.GetCityDistrictName(),
            Code = catottg.Code
        };
}
