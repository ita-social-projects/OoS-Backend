using AutoMapper;
using OutOfSchool.BusinessLogic.Models.Exported;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;

namespace OutOfSchool.BusinessLogic.Util.Mapping;

public class ExternalExportMappingProfile : Profile
{
    public ExternalExportMappingProfile()
    {
        CreateMap<Address, AddressInfoDto>()
            .ForMember(dest => dest.CodeficatorAddress, opt => opt.MapFrom(src => src.CATOTTG));

        CreateMap<CATOTTG, CodeficatorAddressInfoDto>()
            .ForMember(dest => dest.Settlement,
                opt => opt.MapFrom(src => CatottgAddressExtensions.GetSettlementName(src)))
            .ForMember(dest => dest.TerritorialCommunity,
                opt => opt.MapFrom(src => CatottgAddressExtensions.GetTerritorialCommunityName(src)))
            .ForMember(dest => dest.District, opt => opt.MapFrom(src => CatottgAddressExtensions.GetDistrictName(src)))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => CatottgAddressExtensions.GetRegionName(src)))
            .ForMember(dest => dest.CityDistrict,
                opt => opt.MapFrom(src => CatottgAddressExtensions.GetCityDistrictName(src)));

        CreateMap<Workshop, WorkshopInfoBaseDto>();

        CreateMap<WorkshopDescriptionItem, WorkshopDescriptionItemInfo>()
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom(wdi => wdi.SectionName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(wdi => wdi.Description));

        CreateMap<Workshop, WorkshopInfoDto>()
            .IncludeBase<Workshop, WorkshopInfoBaseDto>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => src.Keywords.Split(Constants.MappingSeparator, StringSplitOptions.None)))
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.MapFrom(src => src.InstitutionHierarchy.Title))
            .ForMember(
                dest => dest.DirectionIds,
                opt => opt.MapFrom(
                    src => src.InstitutionHierarchy.Directions.Where(x => !x.IsDeleted).Select(d => d.Id)))
            .ForMember(dest => dest.SubDirectionId,
                opt => opt.MapFrom(
                    src => src.InstitutionHierarchy.Id))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Teachers.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.DateTimeRanges,
                opt => opt.MapFrom(src => src.DateTimeRanges.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.WorkshopDescriptionItems,
                opt => opt.MapFrom(src => src.WorkshopDescriptionItems.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.PayRate, opt => opt.MapFrom(src => src.PayRate))
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(x => x.Name)))
            .ForMember(dest => dest.LanguageOfEducation, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore());

        CreateMap<Provider, ProviderInfoBaseDto>();

        CreateMap<Provider, ProviderInfoDto>()
            .IncludeBase<Provider, ProviderInfoBaseDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.Name))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution.Title))
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)))
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore());

        CreateMap<ProviderSectionItem, ProviderSectionItemInfoDto>()
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom(psi => psi.Name));

        CreateMap<Direction, DirectionInfoBaseDto>();

        CreateMap<Direction, DirectionInfoDto>()
            .IncludeBase<Direction, DirectionInfoBaseDto>();

        CreateMap<InstitutionHierarchy, SubDirectionsInfoBaseDto>();

        CreateMap<InstitutionHierarchy, SubDirectionsInfoDto>()
            .IncludeBase<InstitutionHierarchy, SubDirectionsInfoBaseDto>()
            .ForMember(dest => dest.DirectionIds, opt => opt.MapFrom(src => src.Directions.Select(x => x.Id)));

        CreateMap<Teacher, TeacherInfoDto>()
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty));
    }
}