using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models;

public static class CatottgAddressExtensions
{
    public static string GetCityDistrictName(this CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Name : null;

    public static string GetSettlementName(this CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent?.Name : src.Name;

    public static string GetTerritorialCommunityName(this CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent?.Parent?.Name : src.Parent?.Name;

    public static string GetDistrictName(this CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent?.Parent?.Parent?.Name : src.Parent?.Parent?.Name;

    public static string GetRegionName(this CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent?.Parent?.Parent?.Parent?.Name : src.Parent?.Parent?.Parent?.Name;
}