using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models;

public static class CatottgAddressExtensions
{
    public static string GetCityDistrictName(CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Name : null;

    public static string GetSettlementName(CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent.Name : src.Name;

    public static string GetTerritorialCommunityName(CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent?.Parent?.Name : src.Parent?.Name;

    public static string GetDistrictName(CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent?.Parent?.Parent.Name : src.Parent?.Parent.Name;

    public static string GetRegionName(CATOTTG src)
        => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent?.Parent?.Parent?.Parent.Name : src.Parent?.Parent?.Parent.Name;
}