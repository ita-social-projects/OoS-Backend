using Nest;

namespace OutOfSchool.ElasticsearchData.Models;

public class AddressES
{
    public long Id { get; set; }

    public string City { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public long CATOTTGId { get; set; }

    public CodeficatorAddressES CodeficatorAddressES { get; set; }

    public string Street { get; set; }

    public string BuildingNumber { get; set; }

    [GeoPoint]
    public GeoLocation Point { get; set; }
}