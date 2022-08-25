using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Models.Geocoding;

public class GeocodingListFeatureResponse : GeocodingApiResponse
{
    [JsonProperty("type")]
    public override string Type { get; } = "FeatureCollection";

    [JsonProperty("features")]
    public List<GeocodingSingleFeatureResponse> Features { get; set; } = new ();
}
