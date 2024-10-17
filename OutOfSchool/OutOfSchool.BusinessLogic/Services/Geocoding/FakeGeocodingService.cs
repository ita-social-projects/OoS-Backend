using AutoMapper;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Models.Geocoding;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

public class FakeGeocodingService(
    ICodeficatorService codeficatorService,
    IMapper mapper,
    ILogger<GeocodingService> logger)
    : IGeocodingService
{
    private readonly ICodeficatorService codeficatorService = codeficatorService ?? throw new ArgumentNullException(nameof(codeficatorService));
    private readonly IMapper mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private ILogger<GeocodingService> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Either<ErrorResponse, GeocodingResponse>> GetGeocodingInfo(GeocodingRequest request)
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
            logger.LogError(e, "Unable to retrieve codeficator");
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

        return new GeocodingResponse
        {
            CATOTTGId = request.CATOTTGId,
            Codeficator = address,
            Lat = lat,
            Lon = lon,
            Street = request.Street,
            BuildingNumber = request.BuildingNumber,
        };
    }

    public async Task<Either<ErrorResponse, GeocodingResponse>> GetReverseGeocodingInfo(GeocodingRequest request)
    {
        if (request is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
            };
        }

        try
        {
            var catottg = await codeficatorService
                .GetNearestByCoordinates(request.Lat, request.Lon);

            if (catottg is null)
            {
                return new ErrorResponse
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = "No city data available",
                };
            }

            return new GeocodingResponse
            {
                Codeficator = mapper.Map<CodeficatorAddressDto>(catottg),
                CATOTTGId = catottg.Id,
                Street = "вул. Тестова",
                BuildingNumber = "1",
                Lat = request.Lat,
                Lon = request.Lon,
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to retrieve codeficator");
            return new ErrorResponse
            {
                Message = "Unable to retrieve codeficator data",
            };
        }
    }
}