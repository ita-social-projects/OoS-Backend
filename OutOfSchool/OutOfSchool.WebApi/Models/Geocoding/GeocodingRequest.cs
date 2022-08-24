namespace OutOfSchool.WebApi.Models.Geocoding;

public class GeocodingRequest
{
    public long CATOTTGId { get; set; } = default;

    public string Street { get; set; } = string.Empty;

    public string BuildingNumber { get; set; } = string.Empty;

    public double Lat { get; set; } = default;

    public double Lon { get; set; } = default;

    public bool IsReverse { get; set; }
}