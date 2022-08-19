using JsonSubTypes;
using Newtonsoft.Json;
using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Models.Geocoding;

[JsonConverter(typeof(JsonSubtypes), "type")]
[JsonSubtypes.KnownSubType(typeof(GeocodingSingleFeatureResponse), "Feature")]
[JsonSubtypes.KnownSubType(typeof(GeocodingListFeatureResponse), "FeatureCollection")]
[JsonSubtypes.FallBackSubType(typeof(GeocodingEmptyResponse))]
public class GeocodingApiResponse : IResponse
{
    public virtual string Type { get; }
}