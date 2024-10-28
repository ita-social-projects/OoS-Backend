using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Geocoding;

public class GeocodingListFeatureResponse : GeocodingApiResponse
{
    [JsonPropertyName("type")]
    public override string Type { get; } = "FeatureCollection";

    [JsonPropertyName("features")]
    public List<GeocodingSingleFeatureResponse> Features { get; set; } = new();
}
