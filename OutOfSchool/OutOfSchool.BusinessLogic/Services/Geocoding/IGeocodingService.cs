#nullable enable

using OutOfSchool.BusinessLogic.Models.Geocoding;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for geocoding functionality.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Get coordinates by address.
    /// </summary>
    /// <param name="request">Geocoding request. </param>
    /// <returns>The task result contains the <see cref="Either{ErrorResponse, GeocodingResponse}"/>.</returns>
    public Task<Either<ErrorResponse, GeocodingResponse?>> GetGeocodingInfo(GeocodingRequest? request);

    /// <summary>
    /// Get address by coordinates.
    /// </summary>
    /// <param name="request">Geocoding request. </param>
    /// <returns>The task result contains the <see cref="Either{ErrorResponse, GeocodingResponse}"/>.</returns>
    public Task<Either<ErrorResponse, GeocodingResponse?>> GetReverseGeocodingInfo(
        GeocodingRequest? request);
}