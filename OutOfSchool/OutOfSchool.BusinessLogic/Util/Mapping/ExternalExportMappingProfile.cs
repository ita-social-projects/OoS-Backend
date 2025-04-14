using AutoMapper;
using OutOfSchool.BusinessLogic.Models.Exported.CompetitiveEvents;
using OutOfSchool.BusinessLogic.Models.Exported.Contacts;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Util.Mapping;

public class ExternalExportMappingProfile : Profile
{
    public ExternalExportMappingProfile()
    {
        CreateMap<Contacts, ContactsInfoDto>();
        CreateMap<ContactsAddress, AddressInfoDto>()
            .ForMember(dest => dest.CodeficatorAddress, opt => opt.MapFrom(src => src.CATOTTG));
        CreateMap<PhoneNumber, PhoneNumberInfoDto>();
        CreateMap<Email, EmailInfoDto>();
        CreateMap<SocialNetwork, SocialNetworkInfoDto>();

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
                    src => src.InstitutionHierarchy.SubDirections.Where(x => !x.IsDeleted && !x.Direction.IsDeleted).Select(d => d.DirectionId)))
            .ForMember(dest => dest.SubDirectionIds,
                opt => opt.MapFrom(
                    src => src.InstitutionHierarchy.SubDirections.Where(x => !x.IsDeleted).Select(x => x.Id)))
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
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts));

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

        CreateMap<SubDirection, SubDirectionsInfoBaseDto>();

        CreateMap<SubDirection, SubDirectionsInfoDto>()
            .IncludeBase<SubDirection, SubDirectionsInfoBaseDto>();

        CreateMap<Teacher, TeacherInfoDto>()
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty));

        CreateMap<CompetitiveEventCoverage, CoverageInfoDto>();
        CreateMap<CompetitiveEventAccountingType, AccountingTypeInfoDto>();
        CreateMap<CompetitiveEventDescriptionItem, CompetitiveEventDescriptionItemInfoDto>();
        CreateMap<CompetitiveEvent, CompetitiveEventInfoDto>()
            .ForMember(dest => dest.ParentEventId, opt => opt.MapFrom(src => src.ParentId))
            .ForMember(dest => dest.SubDirections,
                opt => opt.MapFrom(src => string.Join(',', src.SubDirections.Select(s => s.Title))))
            .ForMember(dest => dest.SubDirectionIds,
                opt => opt.MapFrom(src => src.SubDirections.Where(x => !x.IsDeleted).Select(d => d.Id)))
            .ForMember(dest => dest.CompetitiveSelectionDescription, opt => opt.MapFrom(src => src.AdditionalDescription))
            .ForMember(dest => dest.AccountingType, opt => opt.MapFrom(src => src.CompetitiveEventAccountingType))
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts));
    }
}