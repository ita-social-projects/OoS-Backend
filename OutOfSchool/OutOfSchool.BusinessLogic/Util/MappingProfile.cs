using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Achievement;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Models.BlockedProviderParent;
using OutOfSchool.BusinessLogic.Models.Changes;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.Geocoding;
using OutOfSchool.BusinessLogic.Models.Notifications;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.Exported;
using OutOfSchool.BusinessLogic.Models.SocialGroup;
using OutOfSchool.BusinessLogic.Models.StatisticReports;
using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Util.CustomComparers;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.BusinessLogic.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Workshop, WorkshopBaseDto>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => src.Keywords.Split(Constants.MappingSeparator, StringSplitOptions.None)))
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.MapFrom(src => src.InstitutionHierarchy.Title))
            .ForMember(
                dest => dest.DirectionIds,
                opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions.Where(x => !x.IsDeleted).Select(d => d.Id)))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(dest => dest.ProviderLicenseStatus, opt => opt.MapFrom(src => src.Provider.LicenseStatus))
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Teachers.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.DateTimeRanges, opt => opt.MapFrom(src => src.DateTimeRanges.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.WorkshopDescriptionItems, opt => opt.MapFrom(src => src.WorkshopDescriptionItems.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.IncludedStudyGroups, opt => opt.Ignore());

        CreateSoftDeletedMap<WorkshopBaseDto, Workshop>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => string.Join(Constants.MappingSeparator, src.Keywords.Distinct())))
            .ForMember(dest => dest.DateTimeRanges, opt => opt.MapFrom((dto, entity, dest, ctx) =>
            {
                var dateTimeRanges = ctx.Mapper.Map<List<DateTimeRange>>(dto.DateTimeRanges);
                if (dest is { } && dest.Any())
                {
                    var dtoTimeRangesHs =
                        new HashSet<DateTimeRange>(dateTimeRanges, new DateTimeRangeComparerWithoutFK());
                    foreach (var destDateTimeRange in dest)
                    {
                        if (dtoTimeRangesHs.Remove(destDateTimeRange))
                        {
                            dtoTimeRangesHs.Add(destDateTimeRange);
                        }
                    }

                    return dtoTimeRangesHs.ToList();
                }

                return dateTimeRanges;
            }))

            .ForMember(dest => dest.Teachers, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.Applications, opt => opt.Ignore())
            .ForMember(dest => dest.ChatRooms, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderOwnership, opt => opt.Ignore())
            .ForMember(dest => dest.Document, opt => opt.Ignore())
            .ForMember(dest => dest.File, opt => opt.Ignore())
            .ForMember(dest => dest.IsSystemProtected, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.ParentWorkshop, opt => opt.Ignore())
            .ForMember(dest => dest.IncludedStudyGroups, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderTitle, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderTitleEn, opt => opt.Ignore());

        CreateSoftDeletedMap<WorkshopCreateRequestDto, Workshop>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => string.Join(Constants.MappingSeparator, src.Keywords.Distinct())))
            .ForMember(dest => dest.DateTimeRanges, opt => opt.MapFrom((dto, entity, dest, ctx) =>
            {
                var dateTimeRanges = ctx.Mapper.Map<List<DateTimeRange>>(dto.DateTimeRanges);
                if (dest is { } && dest.Any())
                {
                    var dtoTimeRangesHs =
                        new HashSet<DateTimeRange>(dateTimeRanges, new DateTimeRangeComparerWithoutFK());
                    foreach (var destDateTimeRange in dest)
                    {
                        if (dtoTimeRangesHs.Remove(destDateTimeRange))
                        {
                            dtoTimeRangesHs.Add(destDateTimeRange);
                        }
                    }

                    return dtoTimeRangesHs.ToList();
                }

                return dateTimeRanges;
            }))

            .ForMember(dest => dest.Teachers, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.Applications, opt => opt.Ignore())
            .ForMember(dest => dest.ChatRooms, opt => opt.Ignore())
            .ForMember(dest => dest.Document, opt => opt.Ignore())
            .ForMember(dest => dest.File, opt => opt.Ignore())
            .ForMember(dest => dest.IsSystemProtected, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeleteDate, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderTitle, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderTitleEn, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderOwnership, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.Ignore())
            .ForMember(dest => dest.ParentWorkshop, opt => opt.Ignore())
            .ForMember(dest => dest.IncludedStudyGroups, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveFrom, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveTo, opt => opt.Ignore());

        CreateMap<Workshop, WorkshopCreateRequestDto>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => src.Keywords.Split(Constants.MappingSeparator, StringSplitOptions.None)))
            .ForMember(
                dest => dest.DirectionIds,
                opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions.Where(x => !x.IsDeleted).Select(d => d.Id)))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Teachers.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.DateTimeRanges, opt => opt.MapFrom(src => src.DateTimeRanges.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.WorkshopDescriptionItems, opt => opt.MapFrom(src => src.WorkshopDescriptionItems.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.Tags.Select(tag => tag.Id).ToList()));

        CreateMap<WorkshopV2CreateRequestDto, Workshop>()
            .IncludeBase<WorkshopCreateRequestDto, Workshop>()
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore());

        CreateMap<Workshop, WorkshopV2CreateRequestDto>()
            .IncludeBase<Workshop, WorkshopCreateRequestDto>()
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(w => w.ExternalStorageId).ToList()))
            .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(src => src.CoverImageId))
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore());

        CreateMap<Workshop, WorkshopDto>()
            .IncludeBase<Workshop, WorkshopBaseDto>()
            .ForMember(dest => dest.TakenSeats, opt => opt.MapFrom(src => src.Applications.TakenSeats()))
            .IncludeBase<object, IHasRating>()
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.Provider.IsBlocked))
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(w => w.ExternalStorageId).ToList()))
            .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(src => src.CoverImageId))
            .ForMember(dest => dest.ProviderStatus, opt => opt.MapFrom(src => src.Provider.Status))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags))
            .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.Tags.Select(tag => tag.Id)));

        CreateMap<WorkshopDto, Workshop>()
            .IncludeBase<WorkshopBaseDto, Workshop>();

        CreateMap<Workshop, WorkshopV2Dto>()
            .IncludeBase<Workshop, WorkshopDto>()
            .Apply(IgnoreAllImages);

        CreateMap<WorkshopV2Dto, Workshop>()
            .IncludeBase<WorkshopDto, Workshop>();

        CreateMap<Workshop, WorkshopCreateUpdateDto>()
            .IncludeBase<Workshop, WorkshopBaseDto>()
            .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.Tags.Select(tag => tag.Id).ToList()));

        CreateMap<WorkshopDto, WorkshopCreateUpdateDto>()
            .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.Tags.Select(tag => tag.Id).ToList()));

        CreateMap<WorkshopCreateUpdateDto, Workshop>()
            .IncludeBase<WorkshopBaseDto, Workshop>()
            .ForMember(dest => dest.Tags, opt => opt.Ignore());

        CreateMap<WorkshopCreateUpdateDto, WorkshopDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.TagIds.Select(id => new TagDto { Id = id }).ToList()))
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderOwnership, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderStatus, opt => opt.Ignore());

        CreateMap<WorkshopCreateUpdateDto, IHasRating>();

        CreateMap<WorkshopDescriptionItem, WorkshopDescriptionItemDto>().ReverseMap();

        CreateMap<WorkshopStatusDto, WorkshopStatusWithTitleDto>()
            .ForMember(dest => dest.Title, opt => opt.Ignore());

        CreateMap<WorkshopStatusWithTitleDto, WorkshopStatusDto>();

        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.CodeficatorAddressDto, opt => opt.MapFrom(src => src.CATOTTG));

        CreateMap<Address, AddressInfoDto>()
             .ForMember(dest => dest.CodeficatorAddress, opt => opt.MapFrom(src => src.CATOTTG));

        /// <summary>
        /// The localization is done outside the mapping
        /// Original code:
        /// CreateMap<Tag, TagDto>()
        ///     .ForMember(dest => dest.Name, opt => opt.MapFrom((src, dest, destMember, context) =>
        ///     context.Items.ContainsKey("Localization") &&
        ///     context.Items["Localization"] is LocalizationType loc &&
        ///     loc == LocalizationType.En ? src.NameEn : src.Name))
        ///     src.Name));
        /// </summary>
        CreateMap<Tag, TagDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        CreateMap<TagCreate, Tag>()
            .ForMember(dest => dest.Workshops, opt => opt.Ignore());

        /// <summary>
        /// The localization is done outside the mapping
        /// Original code:
        /// CreateMap<SocialGroup, SocialGroupDto>()
        ///     .ForMember(dest => dest.Name, opt => opt.MapFrom((src, dest, destMembet, context) =>
        ///     context.Items.ContainsKey("Localization") &&
        ///     context.Items["Localization"] is LocalizationType loc &&
        ///     loc == LocalizationType.En ? src.NameEn : src.Name))
        ///     src.Name));
        /// </summary>
        CreateMap<SocialGroup, SocialGroupDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        CreateMap<SocialGroup, SocialGroupCreate>();

        CreateMap<SocialGroupCreate, SocialGroup>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore());

        CreateSoftDeletedMap<AddressDto, Address>()
            .ForMember(dest => dest.CATOTTG, opt => opt.Ignore())
            .ForMember(dest => dest.GeoHash, opt => opt.Ignore());

        CreateSoftDeletedMap<BlockedProviderParentBlockDto, BlockedProviderParent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserIdBlock, opt => opt.Ignore())
            .ForMember(dest => dest.UserIdUnblock, opt => opt.Ignore())
            .ForMember(dest => dest.DateTimeFrom, opt => opt.Ignore())
            .ForMember(dest => dest.DateTimeTo, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore());

        CreateMap<BlockedProviderParent, BlockedProviderParentDto>().ReverseMap();

        CreateMap<ProviderSectionItem, ProviderSectionItemDto>()
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom(psi => psi.Name));

        CreateSoftDeletedMap<ProviderSectionItemDto, ProviderSectionItem>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(psi => psi.SectionName))
            .ForMember(dest => dest.Provider, opt => opt.Ignore());

        CreateMap<ProviderSectionItem, ProviderSectionItemInfoDto>()
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom(psi => psi.Name));

        CreateMap<ProviderType, ProviderTypeDto>().ReverseMap();

        CreateMap<Provider, ProviderDto>()
            .Apply(AddCommonProvider2ProviderBaseDto)
            .IncludeBase<object, IHasRating>()
            .ForMember(dest => dest.ProviderSectionItems, opt => opt.MapFrom(src => src.ProviderSectionItems.Where(x => !x.IsDeleted)));

        CreateSoftDeletedMap<ProviderDto, Provider>()
            .Apply(IgnoreCommonProviderBaseDto2Provider)
            .ForMember(dest => dest.Workshops, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.Positions, opt => opt.Ignore());

        CreateSoftDeletedMap<ProviderUpdateDto, Provider>()
            .Apply(IgnoreCommonProviderBaseDto2Provider)
            .ForMember(dest => dest.Ownership, opt => opt.Ignore())
            .ForMember(dest => dest.Workshops, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.BlockPhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.BlockReason, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Positions, opt => opt.Ignore());

        CreateMap<Provider, ProviderUpdateDto>()
            .Apply(AddCommonProvider2ProviderBaseDto);

        CreateMap<Provider, ProviderCsvDto>()
            .ForMember(dest => dest.Ownership, opt => opt.MapFrom((dest, src) => dest.Ownership switch
            {
                OwnershipType.State => "Державна",
                OwnershipType.Common => "Комунальна",
                OwnershipType.Private => "Приватна",
                _ => string.Empty,
            }))
            .ForMember(dest => dest.License, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.License) ? "не вказано" : src.License))
            .ForMember(dest => dest.Settlement, opt => opt.MapFrom(src => CatottgAddressExtensions.GetSettlementName(src.LegalAddress.CATOTTG)))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => $"{src.LegalAddress.Street}, {src.LegalAddress.BuildingNumber}"));

        CreateMap<ProviderDto, ProviderUpdateDto>();

        CreateSoftDeletedMap<ProviderCreateDto, Provider>()
            .Apply(IgnoreCommonProviderBaseDto2Provider)
            .ForMember(dest => dest.Workshops, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore())
            .ForMember(dest => dest.BlockPhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.BlockReason, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Positions, opt => opt.Ignore());

        CreateMap<Provider, ProviderCreateDto>()
            .Apply(AddCommonProvider2ProviderBaseDto);

        CreateMap<ProviderCreateDto, ProviderDto>()
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.BlockReason, opt => opt.Ignore())
            .IncludeBase<object, IHasRating>()
            .ForMember(dest => dest.BlockPhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.Ignore());

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
                dest => dest.Directions,
                opt => opt.MapFrom(
                    src => src.InstitutionHierarchy.Directions.Where(x => !x.IsDeleted).Select(d => d.Title)))
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

        CreateSoftDeletedMap<TeacherDTO, Teacher>()
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopId, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Workshop, opt => opt.Ignore())
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty));

        CreateMap<Teacher, TeacherDTO>()
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty));

        CreateMap<Teacher, TeacherInfoDto>()
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty));

        CreateMap<DateTimeRange, DateTimeRangeDto>()
            .ForMember(dtr => dtr.Workdays, cfg => cfg.MapFrom(dtr => dtr.Workdays.ToDaysBitMaskEnumerable().ToList()));

        CreateSoftDeletedMap<DateTimeRangeDto, DateTimeRange>()
            .ForMember(dtr => dtr.Workdays, cfg => cfg.MapFrom(dtr => dtr.Workdays.ToDaysBitMask()))
            .ForMember(dest => dest.WorkshopId, opt => opt.Ignore());

        CreateMap<Application, ApplicationDto>();
        CreateMap<ApplicationCreate, Application>()
            .ConvertUsing(src => new Application
            {
                ChildId = src.ChildId,
                ParentId = src.ParentId,
                WorkshopId = src.WorkshopId,
            });

        CreateSoftDeletedMap<ApplicationDto, Application>()
            .ForMember(dest => dest.Workshop, opt => opt.Ignore());

        CreateMap<Workshop, WorkshopCard>()
            .IncludeBase<Workshop, WorkshopBaseCard>()
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .IncludeBase<object, IHasRating>()
            .ForMember(dest => dest.TakenSeats, opt => opt.MapFrom(src => src.Applications.TakenSeats()));

        CreateMap<Workshop, WorkshopBaseCard>()
            .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
            .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(s => s.CoverImageId))
            .ForMember(
                dest => dest.DirectionIds,
                opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions.Where(x => !x.IsDeleted).Select(x => x.Id)))
            .IncludeBase<object, IHasRating>()
            .ForMember(dest => dest.ProviderLicenseStatus, opt =>
                opt.MapFrom(src => src.Provider.LicenseStatus));

        _ = CreateMap<Workshop, WorkshopProviderViewCard>()
            .IncludeBase<Workshop, WorkshopBaseCard>()
            .ForMember(dest => dest.AmountOfPendingApplications, opt => opt.MapFrom(src => src.Applications.AmountOfPendingApplications()))
            .ForMember(dest => dest.TakenSeats, opt => opt.MapFrom(src => src.Applications.TakenSeats()))
            .ForMember(dest => dest.UnreadMessages, opt => opt.Ignore());

        CreateMap<Child, ChildDto>()
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty))
            .ForMember(dest => dest.SocialGroups, opt => opt.MapFrom(src => src.SocialGroups.Where(x => !x.IsDeleted)));

        CreateSoftDeletedMap<ChildDto, Child>()
            .ForMember(c => c.Parent, m => m.Ignore())
            .ForMember(c => c.Achievements, m => m.Ignore())
            .ForMember(c => c.SocialGroups, m => m.Ignore());

        CreateSoftDeletedMap<ChildCreateDto, Child>()
            .ForMember(c => c.Id, m => m.Ignore())
            .ForMember(c => c.Parent, m => m.Ignore())
            .ForMember(c => c.Achievements, m => m.Ignore())
            .ForMember(c => c.SocialGroups, m => m.Ignore());

        CreateSoftDeletedMap<ChildUpdateDto, Child>()
            .ForMember(c => c.Id, m => m.Ignore())
            .ForMember(c => c.Parent, m => m.Ignore())
            .ForMember(c => c.Achievements, m => m.Ignore())
            .ForMember(c => c.SocialGroups, m => m.Ignore());

        CreateMap<Parent, ParentDTO>().ReverseMap();

        CreateMap<ParentCreateDto, Parent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.ChatRooms, opt => opt.Ignore());

        CreateMap<Parent, ParentDtoWithContactInfo>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(s => s.User.Email))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(s => s.User.EmailConfirmed))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(s => s.User.PhoneNumber))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(s => s.User.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(s => s.User.MiddleName ?? string.Empty))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(s => s.User.FirstName))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(s => s.User.IsBlocked));

        CreateMap<CompanyInformationItem, CompanyInformationItemDto>().ReverseMap();
        CreateMap<CompanyInformation, CompanyInformationDto>().ReverseMap();

        CreateMap<InstitutionHierarchy, InstitutionHierarchyDto>();
        CreateSoftDeletedMap<InstitutionHierarchyDto, InstitutionHierarchy>()
            .ForMember(c => c.Parent, m => m.Ignore())
            .ForMember(c => c.Directions, m => m.Ignore())
            .ForMember(c => c.Institution, m => m.Ignore());

        CreateMap<Institution, InstitutionDto>().ReverseMap();
        CreateMap<InstitutionFieldDescription, InstitutionFieldDescriptionDto>().ReverseMap();

        CreateMap<CATOTTG, CodeficatorDto>();

        CreateMap<Notification, NotificationDto>().ReverseMap()
            .ForMember(n => n.Id, n => n.Ignore());

        CreateMap<OperationWithObject, OperationWithObjectDto>();

        CreateMap<StatisticReport, StatisticReportDto>().ReverseMap();

        CreateMap<ElasticsearchSyncRecord, ElasticsearchSyncRecordDto>().ReverseMap();

#warning The next mapping is here to test UI Admin features. Will be removed or refactored
        CreateMap<ShortUserDto, AdminDto>();

        CreateMap<User, EmployeeDto>()
            .ForMember(dest => dest.AccountStatus, m => m.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

        CreateMap<User, FullEmployeeDto>()
            .IncludeBase<User, EmployeeDto>()
            .ForMember(dest => dest.WorkshopTitles, opt => opt.Ignore())
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty));

        CreateSoftDeletedMap<DirectionDto, Direction>()
            .ForMember(dest => dest.InstitutionHierarchies, opt => opt.Ignore());

        CreateMap<Direction, DirectionDto>()
            .ForMember(dest => dest.WorkshopsCount, opt => opt.Ignore());

        CreateMap<CreateEmployeeDto, CreateProviderAdminRequest>()
            .ForMember(dest => dest.RequestId, opt => opt.Ignore())
            .ForMember(c => c.CreatingTime, m => m.MapFrom(c => Timestamp.FromDateTimeOffset(c.CreatingTime)))
            .ForMember(c => c.ProviderId, m => m.MapFrom(c => c.ProviderId.ToString()))
            .ForMember(c => c.ManagedWorkshopIds, m => m.MapFrom(src => src.ManagedWorkshopIds.Select(id => id.ToString()).ToList()))
            .ForMember(c => c.IsDeputy, m => m.Ignore()); // TODO: remove this property from CreateProviderAdminRequest (generated by GRPC)

        CreateMap<CreateProviderAdminReply, CreateEmployeeDto>()
            .ForMember(c => c.CreatingTime, m => m.MapFrom(c => c.CreatingTime.ToDateTimeOffset()))
            .ForMember(c => c.ProviderId, m => m.MapFrom(c => Guid.Parse(c.ProviderId)))
            .ForMember(c => c.ManagedWorkshopIds, opt => opt.MapFrom(src => src.ManagedWorkshopIds.Select(Guid.Parse).ToList()));

        CreateMap<User, ShortUserDto>()
            .ForMember(dest => dest.Gender, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty));

        CreateSoftDeletedMap<ShortUserDto, User>()
            .IncludeBase<BaseUserDto, User>()
            .ForMember(dest => dest.Email, opt => opt.Ignore());

        CreateMap<IHasUser, BaseUserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.User.MiddleName ?? string.Empty))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
        ;

        CreateMap<Parent, ShortUserDto>()
            .IncludeBase<IHasUser, BaseUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role))
            .ForMember(dest => dest.IsRegistered, opt => opt.MapFrom(src => src.User.IsRegistered))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.User.EmailConfirmed))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

        CreateSoftDeletedMap<BaseUserDto, User>()
            .ForMember(dest => dest.CreatingTime, m => m.Ignore())
            .ForMember(dest => dest.LastLogin, m => m.Ignore())
            .ForMember(dest => dest.IsBlocked, m => m.Ignore())
            .ForMember(dest => dest.Role, m => m.Ignore())
            .ForMember(dest => dest.IsRegistered, m => m.Ignore())
            .ForMember(dest => dest.IsDerived, m => m.Ignore())
            .ForMember(dest => dest.UserName, m => m.Ignore())
            .ForMember(dest => dest.NormalizedEmail, m => m.Ignore())
            .ForMember(dest => dest.NormalizedUserName, m => m.Ignore())
            .ForMember(dest => dest.EmailConfirmed, m => m.Ignore())
            .ForMember(dest => dest.PasswordHash, m => m.Ignore())
            .ForMember(dest => dest.SecurityStamp, m => m.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, m => m.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, m => m.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, m => m.Ignore())
            .ForMember(dest => dest.LockoutEnd, m => m.Ignore())
            .ForMember(dest => dest.LockoutEnabled, m => m.Ignore())
            .ForMember(dest => dest.AccessFailedCount, m => m.Ignore())
            .ForMember(dest => dest.MustChangePassword, m => m.Ignore())
            .ForMember(dest => dest.Individual, m => m.Ignore());

        CreateMap<InstitutionAdmin, MinistryAdminDto>()
            .IncludeBase<IHasUser, BaseUserDto>()
            .ForMember(dest => dest.InstitutionTitle, opt => opt.MapFrom(src => src.Institution.Title))
            .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => AccountStatusExtensions.Convert(src.User)));

        CreateMap<MinistryAdminDto, MinistryAdminBaseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatingTime, opt => opt.Ignore());

        CreateMap<MinistryAdminBaseDto, MinistryAdminDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.AccountStatus, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionTitle, opt => opt.Ignore());

        CreateMap<RegionAdmin, RegionAdminDto>()
            .IncludeBase<IHasUser, BaseUserDto>()
            .ForMember(dest => dest.InstitutionTitle, opt => opt.MapFrom(src => src.Institution.Title))
            .ForMember(dest => dest.CATOTTGCategory, opt => opt.MapFrom(src => src.CATOTTG.Category))
            .ForMember(dest => dest.CATOTTGName, opt => opt.MapFrom(src => src.CATOTTG.Name))
            .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => AccountStatusExtensions.Convert(src.User)));

        CreateMap<RegionAdminDto, RegionAdminBaseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatingTime, opt => opt.Ignore());

        CreateMap<RegionAdminBaseDto, RegionAdminDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.AccountStatus, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionTitle, opt => opt.Ignore())
            .ForMember(dest => dest.CATOTTGCategory, opt => opt.Ignore())
            .ForMember(dest => dest.CATOTTGName, opt => opt.Ignore());

        CreateMap<AreaAdmin, AreaAdminDto>()
            .IncludeBase<IHasUser, BaseUserDto>()
            .ForMember(dest => dest.InstitutionTitle, opt => opt.MapFrom(src => src.Institution.Title))
            .ForMember(dest => dest.CATOTTGCategory, opt => opt.MapFrom(src => src.CATOTTG.Category))
            .ForMember(dest => dest.CATOTTGName, opt => opt.MapFrom(src => src.CATOTTG.Name))
            .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.CATOTTG.Parent.Parent.Id))
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.CATOTTG.Parent.Parent.Name))
            .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => AccountStatusExtensions.Convert(src.User)))
            .AfterMap((src, dest) => dest.RegionName ??= string.Empty);

        CreateMap<AreaAdminDto, AreaAdminBaseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatingTime, opt => opt.Ignore());

        CreateMap<AreaAdminBaseDto, AreaAdminDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.AccountStatus, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionTitle, opt => opt.Ignore())
            .ForMember(dest => dest.CATOTTGCategory, opt => opt.Ignore())
            .ForMember(dest => dest.CATOTTGName, opt => opt.Ignore())
            .ForMember(dest => dest.RegionId, opt => opt.Ignore())
            .ForMember(dest => dest.RegionName, opt => opt.Ignore());

        CreateMap<BaseUserDto, AreaAdminBaseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatingTime, opt => opt.Ignore())
            .ForMember(dest => dest.CATOTTGId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionId, opt => opt.Ignore());

        CreateMap<BaseUserDto, RegionAdminBaseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatingTime, opt => opt.Ignore())
            .ForMember(dest => dest.CATOTTGId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionId, opt => opt.Ignore());

        CreateMap<BaseUserDto, MinistryAdminBaseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatingTime, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionId, opt => opt.Ignore());

        CreateMap<ProviderChangesLogRequest, ChangesLogFilter>()
            .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => "Provider"))
            .ForMember(dest => dest.From, opt => opt.Ignore())
            .ForMember(dest => dest.Size, opt => opt.MapFrom(o => default(int)));

        CreateMap<ApplicationChangesLogRequest, ChangesLogFilter>()
            .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => "Application"))
            .ForMember(dest => dest.From, opt => opt.Ignore())
            .ForMember(dest => dest.Size, opt => opt.MapFrom(o => default(int)));

        CreateMap<AchievementType, AchievementTypeDto>();

        CreateMap<Achievement, AchievementDto>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children.Where(x => !x.IsDeleted)))
            .ForMember(dest => dest.Teachers, opt => opt.MapFrom(src => src.Teachers.Where(x => !x.IsDeleted)));

        CreateSoftDeletedMap<AchievementDto, Achievement>()
            .ForMember(dest => dest.Workshop, opt => opt.Ignore())
            .ForMember(dest => dest.AchievementType, opt => opt.Ignore());

        CreateMap<AchievementTeacher, AchievementTeacherDto>();

        CreateSoftDeletedMap<AchievementTeacherDto, AchievementTeacher>()
            .ForMember(dest => dest.Achievement, opt => opt.Ignore());

        CreateSoftDeletedMap<AchievementCreateDTO, Achievement>()
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Workshop, opt => opt.Ignore())
            .ForMember(dest => dest.AchievementType, opt => opt.Ignore())
            .ForMember(dest => dest.Teachers, opt => opt.Ignore());

        CreateMap<Employee, EmployeeProviderRelationDto>();

        CreateMap<ChatMessageWorkshop, ChatMessageWorkshopDto>().ReverseMap();
        CreateMap<ChatRoomWorkshop, ChatRoomWorkshopDto>();
        CreateMap<Workshop, WorkshopInfoForChatListDto>();
        CreateMap<ChatRoomWorkshopForChatList, ChatRoomWorkshopDtoWithLastMessage>();
        CreateMap<WorkshopInfoForChatList, WorkshopInfoForChatListDto>();

        CreateMap<ParentInfoForChatList, ParentDtoWithContactInfo>()
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName ?? string.Empty));

        CreateMap<ChatMessageInfoForChatList, ChatMessageWorkshopDto>();

        CreateMap<Favorite, FavoriteDto>().ReverseMap();

        CreateMap<ApplicationDto, ParentCard>()
            .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.Workshop.ProviderId))
            .ForMember(dest => dest.ProviderTitle, opt => opt.MapFrom(src => src.Workshop.ProviderTitle))
            .ForMember(dest => dest.ProviderTitleEn, opt => opt.MapFrom(src => src.Workshop.ProviderTitleEn))
            .ForMember(dest => dest.ProviderOwnership, opt => opt.MapFrom(src => src.Workshop.ProviderOwnership))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Workshop.Rating))
            .ForMember(dest => dest.NumberOfRatings, opt => opt.MapFrom(src => src.Workshop.NumberOfRatings))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Workshop.Title))
            .ForMember(dest => dest.ShortTitle, opt => opt.MapFrom(src => src.Workshop.ShortTitle))
            .ForMember(dest => dest.PayRate, opt => opt.MapFrom(src => src.Workshop.PayRate))
            .ForMember(dest => dest.MaxAge, opt => opt.MapFrom(src => src.Workshop.MaxAge))
            .ForMember(dest => dest.MinAge, opt => opt.MapFrom(src => src.Workshop.MinAge))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Workshop.Price))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Workshop.Address))
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchyId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionId, opt => opt.Ignore())
            .ForMember(dest => dest.Institution, opt => opt.Ignore())
            .ForMember(dest => dest.DirectionIds, opt => opt.Ignore())
            .ForMember(dest => dest.WithDisabilityOptions, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableSeats, opt => opt.Ignore())
            .ForMember(dest => dest.TakenSeats, opt => opt.Ignore())
            .ForMember(dest => dest.CompetitiveSelection, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderLicenseStatus, opt => opt.MapFrom(src => src.Workshop.ProviderLicenseStatus))
            .ForMember(dest => dest.FormOfLearning, opt => opt.MapFrom(src => src.Workshop.FormOfLearning));

        CreateMap<Rating, RatingDto>()
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore());

        CreateSoftDeletedMap<RatingDto, Rating>()
            .ForMember(dest => dest.Parent, opt => opt.Ignore());

        CreateMap<AverageRating, AverageRatingDto>();

        CreateMap<InstitutionStatus, InstitutionStatusDTO>().ReverseMap();

        CreateMap<PermissionsForRole, PermissionsForRoleDTO>()
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(c => c.PackedPermissions.UnpackPermissionsFromString()));
        CreateMap<PermissionsForRoleDTO, PermissionsForRole>()
            .ForMember(dest => dest.PackedPermissions, opt => opt.MapFrom(c => c.Permissions.PackPermissionsIntoString()));

        CreateMap<Child, ShortEntityDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.LastName + " " + src.FirstName + " " + src.MiddleName));

        CreateMap<Workshop, ShortEntityDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

        CreateMap<GeocodingSingleFeatureResponse, GeocodingResponse>()
            .ConvertUsing(src => new GeocodingResponse
            {
                Street = $"{src.Properties.StreetType} {src.Properties.Street}",
                BuildingNumber = src.Properties.Name,
                Lon = src.GeoCentroid.Coordinates.FirstOrDefault(),
                Lat = src.GeoCentroid.Coordinates.LastOrDefault(),
            });

        CreateMap<CATOTTG, CodeficatorAddressDto>()
            .ForMember(dest => dest.Settlement, opt => opt.MapFrom(src => CatottgAddressExtensions.GetSettlementName(src)))
            .ForMember(dest => dest.TerritorialCommunity, opt => opt.MapFrom(src => CatottgAddressExtensions.GetTerritorialCommunityName(src)))
            .ForMember(dest => dest.District, opt => opt.MapFrom(src => CatottgAddressExtensions.GetDistrictName(src)))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => CatottgAddressExtensions.GetRegionName(src)))
            .ForMember(dest => dest.CityDistrict, opt => opt.MapFrom(src => CatottgAddressExtensions.GetCityDistrictName(src)));

        CreateMap<CATOTTG, AllAddressPartsDto>()
            .IncludeBase<CATOTTG, CodeficatorAddressDto>()
            .ForMember(dest => dest.AddressParts, opt => opt.MapFrom(src => src));

        CreateMap<CATOTTG, CodeficatorAddressInfoDto>()
            .ForMember(dest => dest.Settlement, opt => opt.MapFrom(src => CatottgAddressExtensions.GetSettlementName(src)))
            .ForMember(dest => dest.TerritorialCommunity, opt => opt.MapFrom(src => CatottgAddressExtensions.GetTerritorialCommunityName(src)))
            .ForMember(dest => dest.District, opt => opt.MapFrom(src => CatottgAddressExtensions.GetDistrictName(src)))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => CatottgAddressExtensions.GetRegionName(src)))
            .ForMember(dest => dest.CityDistrict, opt => opt.MapFrom(src => CatottgAddressExtensions.GetCityDistrictName(src)));

        CreateMap<Provider, ProviderStatusDto>()
            .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.StatusReason, opt => opt.MapFrom(src => string.Empty));

        CreateMap<WorkshopFilter, WorkshopFilterWithSettlements>()
            .ForMember(dest => dest.SettlementsIds, opt => opt.Ignore());

        CreateMap<CompetitiveEvent, CompetitiveEventDto>().ReverseMap();

        CreateMap<CompetitiveEventAccountingType, CompetitiveEventAccountingTypeDto>().ReverseMap();

        CreateMap<CompetitiveEventCoverage, CompetitiveEventCoverageDto>().ReverseMap();

        CreateMap<CompetitiveEventDescriptionItem, CompetitiveEventDescriptionItemDto>().ReverseMap();

        CreateMap<CompetitiveEventRegistrationDeadline, CompetitiveEventRegistrationDeadlineDto>().ReverseMap();
    }

    public IMappingExpression<TSource, TDestination> CreateSoftDeletedMap<TSource, TDestination>()
        where TSource : class
        where TDestination : class, ISoftDeleted
        => CreateMap<TSource, TDestination>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

    private IMappingExpression<Provider, T> AddCommonProvider2ProviderBaseDto<T>(IMappingExpression<Provider, T> mappings)
        where T : ProviderBaseDto
        => mappings
            .ForMember(dest => dest.ActualAddress, opt => opt.MapFrom(src => src.ActualAddress))
            .ForMember(dest => dest.LegalAddress, opt => opt.MapFrom(src => src.LegalAddress))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution))
            .Apply(IgnoreAllImages)
            .Apply(MapImageIds)
        ;

    private IMappingExpression<T, Provider> IgnoreCommonProviderBaseDto2Provider<T>(IMappingExpression<T, Provider> mappings)
        where T : ProviderBaseDto
        => mappings
            .ForMember(dest => dest.Institution, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StatusReason, opt => opt.Ignore())
            .ForMember(dest => dest.LicenseStatus, opt => opt.Ignore())
        ;

    private IMappingExpression<TSource, TDestination> IgnoreAllImages<TSource, TDestination>(IMappingExpression<TSource, TDestination> mappings)
        where TDestination : IHasCoverImage, IHasImages
        => mappings
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
        ;

    private IMappingExpression<TSource, TDestination> MapImageIds<TSource, TDestination>(IMappingExpression<TSource, TDestination> mappings)
        where TSource : class, IHasEntityImages<TSource>
        where TDestination : class, IHasImages
        => mappings
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)))
        ;
}
