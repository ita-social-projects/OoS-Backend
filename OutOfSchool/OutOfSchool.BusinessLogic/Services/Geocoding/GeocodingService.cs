#nullable enable

using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Models.Geocoding;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with geocoding functionality.
/// </summary>
public class GeocodingService(
    IOptions<GeocodingConfig>? options,
    IHttpClientFactory httpClientFactory,
    IOptions<CommunicationConfig> communicationConfig,
    ICodeficatorService? codeficatorService,
    ILogger<GeocodingService> logger
) : CommunicationService(httpClientFactory, communicationConfig, logger), IGeocodingService
{
    private const double SearchBoundsRadiusMeters = 50000.0;
    private readonly GeocodingConfig config = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, GeocodingResponse?>> GetGeocodingInfo(GeocodingRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
            };
        }

        AllAddressPartsDto? address;
        try
        {
            // Get codeficator from DB
            address = await codeficatorService.GetAllAddressPartsById(request.CATOTTGId);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unable to retrieve codeficator");
            return new ErrorResponse
            {
                Message = "Unable to retrieve codeficator data",
            };
        }

        if (address is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "No city data available",
            };
        }

        // If codeficator entry is a city's district - get it's parent (city) coordinates
        var lat = address.AddressParts.Category != CodeficatorCategory.CityDistrict.Name
            ? address.AddressParts.Latitude
            : address.AddressParts.Parent.Latitude;
        var lon = address.AddressParts.Category != CodeficatorCategory.CityDistrict.Name
            ? address.AddressParts.Longitude
            : address.AddressParts.Parent.Longitude;

        // Create bounds to limit the search are to certain coordinates square
        // should help minimize response data for cities & streets with equal name
        // 50km is 2x time more then enough for Kyiv
        var bounds = new RectangularBounds(lat, lon, SearchBoundsRadiusMeters);

        var req = new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Url = new Uri(config.BaseUrl),
            Query = new Dictionary<string, string>
            {
                { "key", config.ApiKey },
                { "categories", "adr_address" },
                { "text", $"{address.AddressParts.Name}, {request.Street}, {request.BuildingNumber}" },
                { "contains", bounds.WKT },
            },
        };

        var response = await SendRequest<GeocodingApiResponse, ErrorResponse>(req);

        return response.Map(r =>
        {
            var result = new GeocodingResponse
            {
                CATOTTGId = request.CATOTTGId,
                Codeficator = address,
            };

            return r switch
            {
                GeocodingSingleFeatureResponse single => single.SetToResponse(result),

                // for now take the most relevant (sorted by API) address,
                // might need to change if frontend has issues
                GeocodingListFeatureResponse multi => multi.Features.Any()
                    ? multi.Features.First().SetToResponse(result)
                    : null,
                GeocodingEmptyResponse => null,
                _ => null
            };
        });
    }

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, GeocodingResponse?>> GetReverseGeocodingInfo(
        GeocodingRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
            };
        }

        IFormatProvider dotSeparatorFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };
        var req = new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Url = new Uri(config.BaseUrl),
            Query = new Dictionary<string, string>
            {
                { "key", config.ApiKey },
                { "categories", "adr_address" },
                { "radius", $"{config.Radius}" },
                { "near", $"{request.Lon.ToString(dotSeparatorFormat)},{request.Lat.ToString(dotSeparatorFormat)}" }, // it has to be lng first, lat second as per api
                { "order", "distance" },
            },
        };
        var response = await SendRequest<GeocodingApiResponse, ErrorResponse>(req).ConfigureAwait(false);
        return await response
            .Map(r => r switch
            {
                GeocodingSingleFeatureResponse single => single.ToResponse(),
                GeocodingListFeatureResponse multi => multi.Features.FirstOrDefault()?.ToResponse(),
                GeocodingEmptyResponse => null,
                _ => null
            })
            .FlatMapAsync<GeocodingResponse?>(async r =>
            {
                if (r is null)
                {
                    // Need cast to convert to Either, instead of setting the actually return null
                    return null as GeocodingResponse;
                }

                try
                {
                    var catottg = await codeficatorService
                        .GetNearestByCoordinates(r.Lat, r.Lon);

                    if (catottg is null)
                    {
                        return new ErrorResponse
                        {
                            HttpStatusCode = HttpStatusCode.BadRequest,
                            Message = "No city data available",
                        };
                    }

                    r.CATOTTGId = catottg.Id;
                    r.Codeficator = catottg.Copy();
                    return r;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Unable to retrieve codeficator");
                    return new ErrorResponse
                    {
                        Message = "Unable to retrieve codeficator data",
                    };
                }
            });
    }
}
