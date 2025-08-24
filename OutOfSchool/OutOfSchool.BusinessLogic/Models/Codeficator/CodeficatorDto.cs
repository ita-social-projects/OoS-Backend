using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models.Codeficator;

public class CodeficatorDto
{
    public long Id { get; set; }

    [MaxLength(20)]
    public string Code { get; set; }

    public long? ParentId { get; set; }

    [MaxLength(3)]
    public string Category { get; set; }

    [MaxLength(30)]
    public string Name { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int Order { get; set; } = default;

    public CodeficatorDto Parent { get; set; }
}

public static class CodeficatorDtoExtensions
{
    public static CodeficatorDto ToCodeficatorDto(this CodeficatorAddressES address)
        => new()
        {
            Id = address.Id,
            ParentId = address.ParentId,
            Category = address.Category,
            Name = address.Settlement,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            Order = address.Order,
            Parent = address.Parent?.ToCodeficatorDto()
        };

    public static CodeficatorDto ToCodeficatorDto(this CATOTTG catottg)
        => new()
        {
            Id = catottg.Id,
            Code = catottg.Code,
            ParentId = catottg.ParentId,
            Category = catottg.Category,
            Name = catottg.Name,
            Latitude = catottg.Latitude,
            Longitude = catottg.Longitude,
            Order = catottg.Order,
            Parent = catottg.Parent?.ToCodeficatorDto()
        };

    public static List<CodeficatorDto> ToCodeficatorDto(this IEnumerable<CATOTTG> list)
        => list.MapToList(ToCodeficatorDto);

    public static CodeficatorAddressDto ToCodeficatorAddressDto(this CATOTTG catottg)
        => new()
        {
            Id = catottg.Id,
            Category = catottg.Category,
            Region = catottg.GetRegionName(),
            District = catottg.GetDistrictName(),
            TerritorialCommunity = catottg.GetTerritorialCommunityName(),
            Settlement = catottg.GetSettlementName(),
            CityDistrict = catottg.GetCityDistrictName(),
            Latitude = catottg.Latitude,
            Longitude = catottg.Longitude,
            Order = catottg.Order,
        };

    public static List<CodeficatorAddressDto> ToCodeficatorAddressDto(this IEnumerable<CATOTTG> list)
        => list.MapToList(ToCodeficatorAddressDto);
}