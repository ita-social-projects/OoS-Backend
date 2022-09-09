namespace OutOfSchool.ElasticsearchData.Models;

public class CodeficatorAddressES
{
    public string FullAddress { get; set; }

    public long Id { get; set; }

    public string Category { get; set; }

    public long? ParentId { get; set; }

    public CodeficatorAddressES Parent { get; set; }

    public string Region { get; set; }

    public string District { get; set; }

    public string TerritorialCommunity { get; set; }

    public string Settlement { get; set; }

    public string CityDistrict { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int Order { get; set; } = default;

    public string FullName { get; set; }
}
