using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models.Geocoding;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class GeocodingController : Controller
{
    private readonly IGeocodingService geocodingService;

    public GeocodingController(IGeocodingService geocodingService)
    {
        this.geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GeocodingResponse>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Geocoding(GeocodingRequest request)
    {
        var result = await Validate(request)
            .FlatMapAsync(async r =>
                r.IsReverse
                    ? await geocodingService.GetReverseGeocodingInfo(r)
                    : await geocodingService.GetGeocodingInfo(r));

        return result.Match<IActionResult>(
            error => error.HttpStatusCode switch
            {
                HttpStatusCode.BadRequest => BadRequest(error.Message),
                _ => StatusCode((int)error.HttpStatusCode),
            },
            r => r is null ? NoContent() : Ok(r));
    }

    private Either<ErrorResponse, GeocodingRequest> Validate(GeocodingRequest request)
    {
        if (request.IsReverse)
        {
            return request.Lat > 0.0 && request.Lon > 0.0 ? request : new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = $"For reverse geocoding {nameof(request.Lat)} and {nameof(request.Lon)} should be more than 0.0",
            };
        }

        return request.CATOTTGId > 0 && !string.IsNullOrEmpty(request.Street) &&
               !string.IsNullOrEmpty(request.BuildingNumber)
            ? request
            : new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = $"For geocoding request {nameof(request.CATOTTGId)}, {nameof(request.Street)}, {nameof(request.BuildingNumber)} should not be empty",
            };
    }
}