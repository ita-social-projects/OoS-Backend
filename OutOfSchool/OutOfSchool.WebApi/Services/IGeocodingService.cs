using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models.Geocoding;

namespace OutOfSchool.WebApi.Services;

public interface IGeocodingService
{
    public Task<Either<ErrorResponse, GeocodingResponse?>> GetGeocodingInfo(GeocodingRequest request);

    public Task<Either<ErrorResponse, GeocodingResponse>> GetReverseGeocodingInfo(
        GeocodingRequest request);
}