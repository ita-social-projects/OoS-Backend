using Newtonsoft.Json;
using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Models.Geocoding;

public class GeocodingResponse : IResponse
{
    public long CATOTTGId { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string BuildingNumber { get; set; }

    public double RefinedLat { get; set; }

    public double RefinedLon { get; set; }
}