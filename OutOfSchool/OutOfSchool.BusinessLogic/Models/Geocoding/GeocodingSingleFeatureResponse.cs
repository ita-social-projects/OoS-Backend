using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Geocoding;

public class GeocodingSingleFeatureResponse : GeocodingApiResponse
{
    [JsonPropertyName("type")]
    public override string Type { get; } = "Feature";

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("properties")]
    public Properties Properties { get; set; }

    [JsonPropertyName("bbox")]
    public List<double> Bbox { get; set; }

    [JsonPropertyName("geo_centroid")]
    public GeoCentroid GeoCentroid { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }
}

public class GeoCentroid
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("coordinates")]
    public List<double> Coordinates { get; set; }
}

public class Properties
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("categories")]
    public string Categories { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    [JsonPropertyName("street_id")]
    public string StreetId { get; set; }

    [JsonPropertyName("lang")]
    public string Lang { get; set; }

    [JsonPropertyName("street")]
    public string Street { get; set; }

    [JsonPropertyName("street_type")]
    public string StreetType { get; set; }

    [JsonPropertyName("settlement_id")]
    public string SettlementId { get; set; }

    [JsonPropertyName("settlement")]
    public string Settlement { get; set; }

    [JsonPropertyName("settlement_type")]
    public string SettlementType { get; set; }

    [JsonPropertyName("copyright")]
    public string Copyright { get; set; }

    [JsonPropertyName("relevance")]
    public double Relevance { get; set; }

    [JsonPropertyName("settlement_url")]
    public Uri SettlementUrl { get; set; }

    [JsonPropertyName("street_url")]
    public Uri StreetUrl { get; set; }
}