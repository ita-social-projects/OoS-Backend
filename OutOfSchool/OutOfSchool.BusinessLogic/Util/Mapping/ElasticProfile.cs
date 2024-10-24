using Elastic.Clients.Elasticsearch;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using Profile = AutoMapper.Profile;

namespace OutOfSchool.BusinessLogic.Util.Mapping;

public class ElasticProfile : Profile
{
    private const double Epsilon = 0.1d;

    public ElasticProfile()
    {
        CreateMap<WorkshopBaseDto, WorkshopES>()
            .IncludeBase<object, IHasRating>()
            .ForMember(
                dest => dest.Keywords,
                opt =>
                    opt.MapFrom(src =>
                        string.Join(Constants.MappingSeparator, src.Keywords.Distinct())))
            .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.DirectionIds))
            .ForMember(
                dest => dest.Description,
                opt =>
                    opt.MapFrom(src =>
                        src.WorkshopDescriptionItems
                            .Aggregate(string.Empty, (accumulator, wdi) =>
                                $"{accumulator}{wdi.SectionName}{Constants.MappingSeparator}{wdi.Description}{Constants.MappingSeparator}")))
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderOwnership, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore());

        CreateMap<WorkshopDto, WorkshopES>()
            .IncludeBase<WorkshopBaseDto, WorkshopES>()
            .CommonFieldsMapping();

        CreateMap<WorkshopV2Dto, WorkshopES>()
            .IncludeBase<WorkshopDto, WorkshopES>()
            .CommonFieldsMapping()
            .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(src => src.CoverImageId));

        CreateMap<AddressDto, AddressES>()
            .ForMember(
                dest => dest.Point,
                opt =>
                    opt.MapFrom(a => GeoLocation.LatitudeLongitude(new LatLonGeoLocation()
                    {
                        Lat = a.Latitude,
                        Lon = a.Longitude,
                    })))
            .ForMember(
                dest => dest.CodeficatorAddressES,
                opt => opt.MapFrom(c => c.CodeficatorAddressDto))
            .ForMember(
                dest => dest.City,
                opt => opt.MapFrom(src => src.CodeficatorAddressDto.Settlement));

        CreateMap<AllAddressPartsDto, CodeficatorAddressES>()
            .ForMember(
                dest => dest.ParentId,
                opt => opt.MapFrom(src => src.AddressParts.ParentId))

            // Ignoring as it was not mapped before
            .ForMember(
                dest => dest.Parent,
                opt => opt.Ignore());

        CreateMap<DateTimeRangeDto, DateTimeRangeES>()
            .ForMember(
                dest => dest.Workdays,
                opt => opt.MapFrom(src => string.Join(' ', src.Workdays)))
            .ForMember(
                dest => dest.WorkshopId,
                opt => opt.Ignore());

        CreateMap<WorkshopFilter, WorkshopFilterES>()
            .ForMember(dest => dest.Workdays, opt => opt.MapFrom(src => string.Join(' ', src.Workdays)))
            .ForMember(dest => dest.ElasticRadius, opt => opt.MapFrom(src => $"{src.RadiusKm * 1000}m"));

        CreateMap<WorkshopES, WorkshopCard>()
            .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
            .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(s => s.CoverImageId))
            .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.DirectionIds));

        CreateMap<SearchResultES<WorkshopES>, SearchResult<WorkshopCard>>();

        CreateMap<AddressES, AddressDto>()
            .ForMember(
                dest => dest.Latitude,
                opt =>
                    opt.MapFrom(src => src.Point.GetLatitude()))
            .ForMember(
                dest => dest.Longitude,
                opt =>
                    opt.MapFrom(src => src.Point.GetLongitude()))
            .ForMember(
                dest => dest.CodeficatorAddressDto,
                opt => opt.MapFrom(src => src.CodeficatorAddressES));

        CreateMap<Address, AddressES>()
            .ForMember(
                dest => dest.Point,
                opt => opt.MapFrom(gl => GeoLocation.LatitudeLongitude(new LatLonGeoLocation()
                {
                    Lat = Math.Abs(gl.Latitude - 0d) < Epsilon ? gl.CATOTTG.Latitude : gl.Latitude,
                    Lon = Math.Abs(gl.Longitude - 0d) < Epsilon ? gl.CATOTTG.Longitude : gl.Longitude,
                })))
            .ForMember(
                dest => dest.City,
                opt => opt.MapFrom(c => c.CATOTTG.Name))
            .ForMember(
                dest => dest.CodeficatorAddressES,
                opt => opt.MapFrom(c => c.CATOTTG));

        CreateMap<CodeficatorAddressES, AllAddressPartsDto>()
            .ForMember(
                dest => dest.AddressParts,
                opt => opt.MapFrom(src => src));

        CreateMap<CodeficatorAddressES, CodeficatorDto>()
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.Settlement))
            .ForMember(
                dest => dest.Code,
                opt => opt.Ignore());

        CreateMap<CATOTTG, CodeficatorAddressES>()
            .ForMember(dest => dest.Settlement, opt => opt.MapFrom(src => CatottgAddressExtensions.GetSettlementName(src)))
            .ForMember(dest => dest.TerritorialCommunity, opt => opt.MapFrom(src => CatottgAddressExtensions.GetTerritorialCommunityName(src)))
            .ForMember(dest => dest.District, opt => opt.MapFrom(src => CatottgAddressExtensions.GetDistrictName(src)))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => CatottgAddressExtensions.GetRegionName(src)))
            .ForMember(dest => dest.CityDistrict, opt => opt.MapFrom(src => CatottgAddressExtensions.GetCityDistrictName(src)))
            .ForMember(dest => dest.FullAddress, opt => opt.Ignore())
            .ForMember(dest => dest.FullName, opt => opt.Ignore());

        CreateMap<DateTimeRange, DateTimeRangeES>()
            .ForMember(
                dest => dest.Workdays,
                opt => opt.MapFrom(dtr => string.Join(" ", dtr.Workdays.ToDaysBitMaskEnumerable())));

        CreateMap<Workshop, WorkshopES>()
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.MapFrom(src => src.InstitutionHierarchy.Title))
            .ForMember(
                dest => dest.DirectionIds,
                opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions.Where(x => !x.IsDeleted).Select(d => d.Id)))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(
                dest => dest.Description,
                opt =>
                    opt.MapFrom(src =>
                        src.WorkshopDescriptionItems.Where(x => !x.IsDeleted)
                            .Aggregate(string.Empty, (accumulator, wdi) =>
                                $"{accumulator}{wdi.SectionName}{Constants.MappingSeparator}{wdi.Description}{Constants.MappingSeparator}")))

            // TODO: Copied this from base MappingProfile but this looks like some messed up lazy loading thing :)
            .ForMember(dest => dest.TakenSeats, opt =>
                opt.MapFrom(src =>
                    src.Applications.Count(x =>
                        !x.IsDeleted && (x.Status == ApplicationStatus.Approved
                        || x.Status == ApplicationStatus.StudyingForYears))));
    }
}