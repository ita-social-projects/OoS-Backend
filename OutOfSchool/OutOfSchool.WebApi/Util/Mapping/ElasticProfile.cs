using Nest;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Codeficator;
using Profile = AutoMapper.Profile;

namespace OutOfSchool.WebApi.Util.Mapping;

public class ElasticProfile : Profile
{
    //TODO: Extract to common constants
    private const char SEPARATOR = '¤';

    public ElasticProfile()
    {
        CreateMap<WorkshopDTO, WorkshopES>()
            .ForMember(
                dest => dest.Keywords,
                opt =>
                    opt.MapFrom(src =>
                        string.Join(SEPARATOR, src.Keywords.Distinct())))
            .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.DirectionIds))
            .ForMember(
                dest => dest.Description,
                opt =>
                    opt.MapFrom(src =>
                        src.WorkshopDescriptionItems
                            .Aggregate(string.Empty, (accumulator, wdi) =>
                                $"{accumulator}{wdi.SectionName}{SEPARATOR}{wdi.Description}{SEPARATOR}")));

        CreateMap<AddressDto, AddressES>()
            .ForMember(
                dest => dest.Point,
                opt =>
                    opt.MapFrom(a => new GeoLocation(a.Latitude, a.Longitude)))
            .ForMember(
                dest => dest.CodeficatorAddressES,
                opt => opt.MapFrom(c => c.CodeficatorAddressDto));
        CreateMap<AllAddressPartsDto, CodeficatorAddressES>()
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.AddressParts.ParentId));
        CreateMap<DateTimeRangeDto, DateTimeRangeES>()
            .ForMember(dest => dest.Workdays, opt => opt.MapFrom(src => string.Join(' ', src.Workdays)));

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
                    opt.MapFrom(src => src.Point.Latitude))
            .ForMember(
                dest => dest.Longitude,
                opt =>
                    opt.MapFrom(src => src.Point.Longitude))
            .ForMember(
                dest => dest.CodeficatorAddressDto,
                opt => opt.MapFrom(src => src.CodeficatorAddressES));

        CreateMap<CodeficatorAddressES, AllAddressPartsDto>()
            .ForMember(
                dest => dest.AddressParts,
                opt => opt.MapFrom(src => src));
        CreateMap<CodeficatorAddressES, CodeficatorDto>();

        CreateMap<Address, AddressES>()
            .ForMember(
                dest => dest.Point,
                opt => opt.MapFrom(gl => new GeoLocation(
                    gl.Latitude == 0d ? gl.CATOTTG.Latitude : gl.Latitude,
                    gl.Longitude == 0d ? gl.CATOTTG.Longitude : gl.Longitude)))
            .ForMember(
                dest => dest.City,
                opt => opt.MapFrom(c => c.CATOTTG.Name))
            .ForMember(
                dest => dest.CodeficatorAddressES,
                opt => opt.MapFrom(c => c.CATOTTG));

        CreateMap<CATOTTG, CodeficatorAddressES>()
            .ForMember(
                dest => dest.Settlement,
                opt => opt.MapFrom(src =>
                    src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent.Name : src.Name))
            .ForMember(
                dest => dest.TerritorialCommunity,
                opt => opt.MapFrom(src =>
                    src.Category == CodeficatorCategory.CityDistrict.Name ? src.Parent.Parent.Name : src.Parent.Name))
            .ForMember(
                dest => dest.District,
                opt => opt.MapFrom(src =>
                    src.Category == CodeficatorCategory.CityDistrict.Name
                        ? src.Parent.Parent.Parent.Name
                        : src.Parent.Parent.Name))
            .ForMember(
                dest => dest.Region,
                opt => opt.MapFrom(src =>
                    src.Category == CodeficatorCategory.CityDistrict.Name
                        ? src.Parent.Parent.Parent.Parent.Name
                        : src.Parent.Parent.Parent.Name))
            .ForMember(
                dest => dest.CityDistrict,
                opt => opt.MapFrom(src => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Name : null))
            .ForMember(dest => dest.FullAddress, opt => opt.Ignore())
            .ForMember(dest => dest.FullName, opt => opt.Ignore());

        CreateMap<DateTimeRange, DateTimeRangeES>()
            .ForMember(
                dest => dest.Workdays,
                opt => opt.MapFrom(dtr => string.Join(" ", dtr.Workdays.ToDaysBitMaskEnumerable())));

        CreateMap<Workshop, WorkshopES>()
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.MapFrom(src => src.InstitutionHierarchy.Title))
            .ForMember(dest => dest.DirectionIds,
                opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions.Select(d => d.Id)))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(
                dest => dest.Description,
                opt =>
                    opt.MapFrom(src =>
                        src.WorkshopDescriptionItems
                            .Aggregate(string.Empty, (accumulator, wdi) =>
                                $"{accumulator}{wdi.SectionName}{SEPARATOR}{wdi.Description}{SEPARATOR}")));
    }
}