using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models.Geocoding;

namespace OutOfSchool.WebApi.Services;

public class GeocodingService : CommunicationService, IGeocodingService
{
    private const double searchBoundsRadiusMeters = 50000.0;
    private readonly GeocodingConfig config;
    private readonly ICodeficatorRepository codeficatorRepository;
    private readonly IMapper mapper;

    public GeocodingService(
        IOptions<GeocodingConfig> options,
        IHttpClientFactory httpClientFactory,
        IOptions<CommunicationConfig> communicationConfig,
        ICodeficatorRepository codeficatorRepository,
        IMapper mapper,
        ILogger<GeocodingService> logger)
        : base(httpClientFactory, communicationConfig.Value, logger)
    {
        config = options.Value;
        this.mapper = mapper;
        this.codeficatorRepository = codeficatorRepository;
    }

    public async Task<Either<ErrorResponse, GeocodingResponse?>> GetGeocodingInfo(GeocodingRequest request)
    {
        try
        {
            // Get codeficator from DB
            var address = await codeficatorRepository.GetById(request.CATOTTGId);

            if (address is null)
            {
                return new ErrorResponse
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = "No city data available",
                };
            }

            // If codeficator entry is a city's district - get it's parent (city) coordinates
            // TODO: check if districts have their own coordinates
            var lat = address.Category != "B" ? address.Latitude : address.Parent.Latitude;
            var lon = address.Category != "B" ? address.Longitude : address.Parent.Longitude;

            // Create bounds to limit the search are to certain coordinates square
            // should help minimize response data for cities & streets with equal name
            // TODO: 50km is 2x time more then enough for Kyiv
            var bounds = new RectangularBounds(lat, lon, searchBoundsRadiusMeters);

            var req = new Request
            {
                HttpMethodType = HttpMethodType.Get,
                Url = new Uri(config.BaseUrl),
                Query = new Dictionary<string, string>
                {
                    {"key", config.ApiKey},
                    {"categories", "adr_address"},
                    {"text", $"{address.Name}, {request.Street}, {request.BuildingNumber}"},
                    { "contains", bounds.WKT },
                },
            };

            var response = await SendRequest<GeocodingApiResponse>(req);

            return await response.MapAsync(async r =>
            {
                var result = new GeocodingResponse();
                result.CATOTTGId = request.CATOTTGId;

                return r switch
                {
                    _ when r is GeocodingSingleFeatureResponse single => mapper.Map(single, result),

                    // TODO: for now take the most relevant (sorted by API) address, might need to change if frontend has issues
                    _ when r is GeocodingListFeatureResponse multi => multi.Features.Any() ? mapper.Map(multi.Features.First(), result) : null,
                    _ when r is GeocodingEmptyResponse => null
                };
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            // TODO: normal error
            return new ErrorResponse();
        }
    }

    public async Task<Either<ErrorResponse, GeocodingResponse>> GetReverseGeocodingInfo(
        GeocodingRequest request)
    {
        var req = new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Url = new Uri(config.BaseUrl),
            Query = new Dictionary<string, string>
            {
                {"key", config.ApiKey},
                {"categories", "adr_address"},
                {"radius", $"{config.Radius}"},
                {"near", $"{request.Lon},{request.Lat}"}, // it has to be lng first, lat second as per api
                {"order", "distance"},
            },
        };
        var response = await SendRequest<GeocodingApiResponse>(req).ConfigureAwait(false);
        return await response
            .Map(r => r switch
            {
                _ when r is GeocodingSingleFeatureResponse single => mapper.Map<GeocodingResponse>(single),
                _ when r is GeocodingListFeatureResponse multi => multi.Features.Select(mapper.Map<GeocodingResponse>).FirstOrDefault(),
                _ when r is GeocodingEmptyResponse => null
            })
            .FlatMapAsync<GeocodingResponse>(async r =>
            {
                if (r is null)
                {
                    // Need cast to convert to Either, instead of setting the actually return null
                    return null as GeocodingResponse;
                }

                try
                {
                    var catottg = await codeficatorRepository
                        .GetNearestByCoordinates(r.RefinedLat, r.RefinedLon);

                    if (catottg is null)
                    {
                        return new ErrorResponse
                        {
                            HttpStatusCode = HttpStatusCode.BadRequest,
                            Message = "No city data available",
                        };
                    }

                    r.CATOTTGId = catottg.Id;
                    return r;
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                    // TODO: normal error
                    return new ErrorResponse();
                }
            });
    }
}

public class RectangularBounds
{
    private const double EarthRadius = 6378.137;

    private double Coef => (1 / ((2 * Math.PI / 360) * EarthRadius)) / 1000;

    private Point northWest;

    private Point northEast;

    private Point southEast;

    private Point southWest;

    public RectangularBounds(double lat, double lon, double deltaMeters)
    {
        northWest = CalculateNorthWest(lat, lon, deltaMeters);
        northEast = CalculateNorthEast(lat, lon, deltaMeters);
        southEast = CalculateSouthEast(lat, lon, deltaMeters);
        southWest = CalculateSouthWest(lat, lon, deltaMeters);
    }

    // Creates a well known text representation of a geometric polygon
    public string WKT =>
        $"POLYGON (( {northWest.lon} {northWest.lat}, {northEast.lon} {northEast.lat}, {southEast.lon} {southEast.lat}, {southWest.lon} {southWest.lat}, {northWest.lon} {northWest.lat} ))";

    private Point CalculateSouthWest(double lat, double lon, double deltaMeters)
    {
        return new Point(lat - (deltaMeters * Coef), lon - (deltaMeters * Coef / Math.Cos(GeoMathHelper.Deg2Rad(lat))));
    }

    private Point CalculateNorthWest(double lat, double lon, double deltaMeters)
    {
        return new Point(lat + (deltaMeters * Coef), lon - (deltaMeters * Coef / Math.Cos(GeoMathHelper.Deg2Rad(lat))));
    }

    private Point CalculateNorthEast(double lat, double lon, double deltaMeters)
    {
        return new Point(lat + (deltaMeters * Coef), lon + (deltaMeters * Coef / Math.Cos(GeoMathHelper.Deg2Rad(lat))));
    }

    private Point CalculateSouthEast(double lat, double lon, double deltaMeters)
    {
        return new Point(lat - (deltaMeters * Coef), lon + (deltaMeters * Coef / Math.Cos(GeoMathHelper.Deg2Rad(lat))));
    }

    private record Point(double lat, double lon);
}