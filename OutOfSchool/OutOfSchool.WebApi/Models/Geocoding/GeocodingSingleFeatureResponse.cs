using Newtonsoft.Json;

namespace OutOfSchool.WebApi.Models.Geocoding;

public class GeocodingSingleFeatureResponse : GeocodingApiResponse
{
    [JsonProperty("type")]
    public override string Type { get; } = "Feature";

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("properties")]
    public Properties Properties { get; set; }

    [JsonProperty("bbox")]
    public List<double> Bbox { get; set; }

    [JsonProperty("geo_centroid")]
    public GeoCentroid GeoCentroid { get; set; }

    [JsonProperty("url")]
    public Uri Url { get; set; }
}

public class GeoCentroid
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("coordinates")]
    public List<double> Coordinates { get; set; }
}

public class Properties
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("categories")]
    public string Categories { get; set; }

    [JsonProperty("country_code")]
    public string CountryCode { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("postal_code")]
    public string PostalCode { get; set; }

    [JsonProperty("street_id")]
    public string StreetId { get; set; }

    [JsonProperty("lang")]
    public string Lang { get; set; }

    [JsonProperty("street")]
    public string Street { get; set; }

    [JsonProperty("street_type")]
    public string StreetType { get; set; }

    [JsonProperty("settlement_id")]
    public string SettlementId { get; set; }

    [JsonProperty("settlement")]
    public string Settlement { get; set; }

    [JsonProperty("settlement_type")]
    public string SettlementType { get; set; }

    [JsonProperty("copyright")]
    public string Copyright { get; set; }

    [JsonProperty("relevance")]
    public double Relevance { get; set; }

    [JsonProperty("settlement_url")]
    public Uri SettlementUrl { get; set; }

    [JsonProperty("street_url")]
    public Uri StreetUrl { get; set; }
}