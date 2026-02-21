using System.Text.Json.Serialization;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models.Geocoding;

[JsonDerivedType(typeof(GeocodingApiResponse), typeDiscriminator: "type")]
[JsonDerivedType(typeof(GeocodingSingleFeatureResponse), typeDiscriminator: "Feature")]
[JsonDerivedType(typeof(GeocodingListFeatureResponse), typeDiscriminator: "FeatureCollection")]
[JsonDerivedType(typeof(GeocodingEmptyResponse))]
public class GeocodingApiResponse : IResponse
{
    public virtual string Type { get; }
}