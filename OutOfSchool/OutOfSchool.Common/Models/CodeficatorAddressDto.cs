using System.Text;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Common.Models;

public class CodeficatorAddressDto
{
    public long Id { get; set; }

    public string Category { get; set; }

    public string Region { get; set; }

    public string District { get; set; }

    public string TerritorialCommunity { get; set; }

    public string Settlement { get; set; }

    public string CityDistrict { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int Order { get; set; }

    public string FullName
    {
        get
        {
            StringBuilder fullName = new StringBuilder();

            if (!string.IsNullOrEmpty(CityDistrict))
            {
                fullName.Append($"{CityDistrict} {CodeficatorCategory.CityDistrict.Abbrivation}");
            }

            if (!string.IsNullOrEmpty(Settlement))
            {
                fullName.Append($"{GetSplitter(fullName)}{Settlement}");
            }

            if (!string.IsNullOrEmpty(TerritorialCommunity))
            {
                fullName.Append($"{GetSplitter(fullName)}{TerritorialCommunity} {CodeficatorCategory.TerritorialCommunity.Abbrivation}");
            }

            if (!string.IsNullOrEmpty(District))
            {
                fullName.Append($"{GetSplitter(fullName)}{District} {CodeficatorCategory.District.Abbrivation}");
            }

            if (!string.IsNullOrEmpty(Region))
            {
                fullName.Append($"{GetSplitter(fullName)}{Region} {CodeficatorCategory.Region.Abbrivation}");
            }

            return fullName.ToString();
        }
    }

    private static string GetSplitter(StringBuilder address) => address.Length == 0 ? string.Empty : Constants.AddressSeparator;
}

public static class CodeficatorAddressDtoExtensions
{
    public static CodeficatorAddressDto Copy(this CodeficatorAddressDto dto)
        => new()
        {
            Id = dto.Id,
            Category = dto.Category,
            Region = dto.Region,
            District = dto.District,
            TerritorialCommunity = dto.TerritorialCommunity,
            Settlement = dto.Settlement,
            CityDistrict = dto.CityDistrict,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Order = dto.Order,
        };
}