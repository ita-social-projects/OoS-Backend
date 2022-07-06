namespace OutOfSchool.Common.Models;

public class CodeficatorAddressDto
{
    public long Id { get; set; }

    public string Category { get; set; }

    public string Region { get; set; }

    public string District { get; set; }

    public string TerritorialCommunity { get; set; }

    public string Settlement { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string FullName
    {
        get
        {
            string addr = Settlement;

            if (!string.IsNullOrEmpty(TerritorialCommunity))
            {
                addr += GetSplitter(addr) + TerritorialCommunity;
            }

            if (!string.IsNullOrEmpty(District))
            {
                addr += GetSplitter(addr) + District;
            }

            if (!string.IsNullOrEmpty(Region))
            {
                addr += GetSplitter(addr) + Region;
            }

            return addr;
        }
    }

    private string GetSplitter(string addr) => string.IsNullOrEmpty(addr) ? string.Empty : Constants.AddressSeparator;
}