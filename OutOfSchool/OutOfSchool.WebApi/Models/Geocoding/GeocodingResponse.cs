using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Models.Geocoding;

public class GeocodingResponse : IResponse
{
    public long CATOTTGId { get; set; }

    public CodeficatorAddressDto Codeficator { get; set; }

    public string Street { get; set; }

    public string BuildingNumber { get; set; }

    public double Lat { get; set; }

    public double Lon { get; set; }
}