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
            StringBuilder addr = new StringBuilder();

            if (!string.IsNullOrEmpty(CityDistrict))
            {
                addr.Append($"{CityDistrict} {CodeficatorCategory.CityDistrict.Abbrivation}");
            }

            if (!string.IsNullOrEmpty(Settlement))
            {
                addr.Append($"{GetSplitter(addr)}{Settlement}");
                //addr += GetSplitter(addr) + Settlement;
            }

            if (!string.IsNullOrEmpty(TerritorialCommunity))
            {
                addr.Append($"{GetSplitter(addr)}{TerritorialCommunity} {CodeficatorCategory.TerritorialCommunity.Abbrivation}");
                //addr += GetSplitter(addr) + TerritorialCommunity;
            }

            if (!string.IsNullOrEmpty(District))
            {
                addr.Append($"{GetSplitter(addr)}{District} {CodeficatorCategory.District.Abbrivation}");
                //addr += GetSplitter(addr) + District;
            }

            if (!string.IsNullOrEmpty(Region))
            {
                addr.Append($"{GetSplitter(addr)}{Region} {CodeficatorCategory.Region.Abbrivation}");
                //addr += GetSplitter(addr) + Region;
            }

            return addr.ToString();
        }
    }

    private string GetSplitter(StringBuilder addr) => addr.Length == 0 ? string.Empty : Constants.AddressSeparator;
}