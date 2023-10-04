using Nest;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Codeficator;
using OutOfSchool.WebApi.Models.Workshops;
using Profile = AutoMapper.Profile;

namespace OutOfSchool.WebApi.Util.Mapping;

public class ElasticProfile : Profile
{
    public ElasticProfile()
    {
        CreateMap<WorkshopBaseDto, WorkshopES>()
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
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore());

        CreateMap<WorkshopDto, WorkshopES>()
            .IncludeBase<WorkshopBaseDto, WorkshopES>();

        CreateMap<WorkshopV2Dto, WorkshopES>()
        .IncludeBase<WorkshopDto, WorkshopES>();

        CreateMap<AddressDto, AddressES>()
            .ForMember(
                dest => dest.Point,
                opt =>
                    opt.MapFrom(a => new GeoLocation(a.Latitude, a.Longitude)))
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
                    opt.MapFrom(src => src.Point.Latitude))
            .ForMember(
                dest => dest.Longitude,
                opt =>
                    opt.MapFrom(src => src.Point.Longitude))
            .ForMember(
                dest => dest.CodeficatorAddressDto,
                opt => opt.MapFrom(src => src.CodeficatorAddressES));

        CreateMap<Address, AddressES>()
            .ForMember(
                dest => dest.Point,
                opt => opt.MapFrom(gl => new Nest.GeoLocation(
                    gl.Latitude == 0d ? gl.CATOTTG.Latitude : gl.Latitude,
                    gl.Longitude == 0d ? gl.CATOTTG.Longitude : gl.Longitude)))
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