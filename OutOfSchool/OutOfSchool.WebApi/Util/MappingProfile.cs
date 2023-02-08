using AutoMapper.Extensions.EnumMapping;
using GrpcService;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Models.BlockedProviderParent;
using OutOfSchool.WebApi.Models.Changes;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Models.Codeficator;
using OutOfSchool.WebApi.Models.Geocoding;
using OutOfSchool.WebApi.Models.Notifications;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.SocialGroup;
using OutOfSchool.WebApi.Models.StatisticReports;
using OutOfSchool.WebApi.Models.SubordinationStructure;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Util.CustomComparers;
using Profile = AutoMapper.Profile;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace OutOfSchool.WebApi.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //TODO: Extract to common constants
        const char SEPARATOR = '¤';
        CreateMap<WorkshopDTO, Workshop>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => string.Join(SEPARATOR, src.Keywords.Distinct())))
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
            .ForMember(dest => dest.ProviderAdmins, opt => opt.Ignore())
            .ForMember(dest => dest.Applications, opt => opt.Ignore())
            .ForMember(dest => dest.ChatRooms, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        CreateMap<Workshop, WorkshopDTO>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => src.Keywords.Split(SEPARATOR, StringSplitOptions.None)))
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)))
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.MapFrom(src => src.InstitutionHierarchy.Title))
            .ForMember(dest => dest.DirectionIds,
                opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions.Select(d => d.Id)))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.TakenSeats, opt =>
                opt.MapFrom(src =>
                    src.Applications.Count(x =>
                        x.Status == ApplicationStatus.Approved
                        || x.Status == ApplicationStatus.StudyingForYears)))
            .ForMember(dest => dest.ProviderLicenseStatus, opt =>
                opt.MapFrom(src => src.Provider.LicenseStatus));

        CreateMap<WorkshopDescriptionItem, WorkshopDescriptionItemDto>().ReverseMap();

        CreateMap<WorkshopStatusDto, WorkshopStatusWithTitleDto>()
            .ForMember(dest => dest.Title, opt => opt.Ignore());

        CreateMap<WorkshopStatusWithTitleDto, WorkshopStatusDto>();

        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.CodeficatorAddressDto, opt => opt.MapFrom(src => src.CATOTTG));

        CreateMap<AddressDto, Address>()
            .ForMember(dest => dest.CATOTTG, opt => opt.Ignore())
            .ForMember(dest => dest.GeoHash, opt => opt.Ignore());

        CreateMap<BlockedProviderParentBlockDto, BlockedProviderParent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserIdBlock, opt => opt.Ignore())
            .ForMember(dest => dest.UserIdUnblock, opt => opt.Ignore())
            .ForMember(dest => dest.DateTimeFrom, opt => opt.Ignore())
            .ForMember(dest => dest.DateTimeTo, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore());

        CreateMap<BlockedProviderParent, BlockedProviderParentDto>().ReverseMap();

        CreateMap<ProviderSectionItem, ProviderSectionItemDto>()
            .ForMember(
                dest =>
                    dest.SectionName, opt =>
                    opt.MapFrom(psi => psi.Name));

        CreateMap<ProviderSectionItemDto, ProviderSectionItem>()
            .ForMember(
                dest =>
                    dest.Name, opt =>
                    opt.MapFrom(psi => psi.SectionName))
            .ForMember(dest => dest.Provider, opt => opt.Ignore());

        CreateMap<ProviderType, ProviderTypeDto>()
            .ReverseMap();
        CreateMap<Provider, ProviderDto>()
            .ForMember(dest => dest.ActualAddress, opt => opt.MapFrom(src => src.ActualAddress))
            .ForMember(dest => dest.LegalAddress, opt => opt.MapFrom(src => src.LegalAddress))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution))
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)));

        CreateMap<ProviderDto, Provider>()
            .ForMember(dest => dest.Workshops, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Institution, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.LicenseStatus, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderAdmins, opt => opt.Ignore());

        CreateMap<TeacherDTO, Teacher>()
            .ForMember(dest => dest.CoverImageId, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopId, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Workshop, opt => opt.Ignore());

        CreateMap<Teacher, TeacherDTO>()
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore());

        CreateMap<DateTimeRange, DateTimeRangeDto>()
            .ForMember(dtr => dtr.Workdays, cfg => cfg.MapFrom(dtr => dtr.Workdays.ToDaysBitMaskEnumerable().ToList()));

        CreateMap<DateTimeRangeDto, DateTimeRange>()
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

        CreateMap<ApplicationDto, Application>().ForMember(dest => dest.Workshop, opt => opt.Ignore());

        CreateMap<Workshop, WorkshopCard>()
            .IncludeBase<Workshop, WorkshopBaseCard>()
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.TakenSeats, opt =>
                opt.MapFrom(src =>
                    src.Applications.Count(x =>
                        x.Status == ApplicationStatus.Approved
                        || x.Status == ApplicationStatus.StudyingForYears)));

        CreateMap<Workshop, WorkshopBaseCard>()
            .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
            .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(s => s.CoverImageId))
            .ForMember(dest => dest.DirectionIds,
                opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions.Select(x => x.Id)))
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderLicenseStatus, opt =>
                opt.MapFrom(src => src.Provider.LicenseStatus));

        CreateMap<Workshop, WorkshopProviderViewCard>()
            .IncludeBase<Workshop, WorkshopBaseCard>()
            .ForMember(dest => dest.AmountOfPendingApplications, opt => opt.MapFrom(src =>
                src.Applications.Count(x =>
                    x.Status == ApplicationStatus.Pending)))
            .ForMember(dest => dest.TakenSeats, opt => opt.MapFrom(src =>
                src.Applications.Count(x =>
                    x.Status == ApplicationStatus.Approved
                    || x.Status == ApplicationStatus.StudyingForYears)))
            .ForMember(dest => dest.Address, opt => opt.Ignore());

        CreateMap<SocialGroup, SocialGroupDto>().ReverseMap();
        CreateMap<SocialGroup, SocialGroupCreate>().ReverseMap();

        CreateMap<Child, ChildDto>();
        CreateMap<ChildDto, Child>()
            .ForMember(c => c.Parent, m => m.Ignore())
            .ForMember(c => c.Achievements, m => m.Ignore())
            .ForMember(c => c.SocialGroups, m => m.Ignore());

        CreateMap<Parent, ParentDTO>().ReverseMap();

        CreateMap<Parent, ParentDtoWithContactInfo>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(s => s.User.Email))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(s => s.User.EmailConfirmed))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(s => s.User.PhoneNumber))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(s => s.User.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(s => s.User.MiddleName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(s => s.User.FirstName));

        CreateMap<CompanyInformationItem, CompanyInformationItemDto>().ReverseMap();
        CreateMap<CompanyInformation, CompanyInformationDto>().ReverseMap();

        CreateMap<InstitutionHierarchy, InstitutionHierarchyDto>().ReverseMap();
        CreateMap<Institution, InstitutionDto>().ReverseMap();
        CreateMap<InstitutionFieldDescription, InstitutionFieldDescriptionDto>().ReverseMap();

        CreateMap<CATOTTG, CodeficatorDto>();

        CreateMap<Notification, NotificationDto>().ReverseMap()
            .ForMember(n => n.Id, n => n.Ignore());

        CreateMap<StatisticReport, StatisticReportDto>().ReverseMap();

        CreateMap<ElasticsearchSyncRecord, ElasticsearchSyncRecordDto>().ReverseMap();

#warning The next mapping is here to test UI Admin features. Will be removed or refactored
        CreateMap<ShortUserDto, AdminDto>();

        CreateMap<User, ProviderAdminDto>()
            .ForMember(dest => dest.IsDeputy, opt => opt.Ignore())
            .ForMember(dest => dest.AccountStatus, m => m.Ignore());

        CreateMap<DirectionDto, Direction>()
            .ForMember(dest => dest.InstitutionHierarchies, opt => opt.Ignore());

        CreateMap<Direction, DirectionDto>()
            .ForMember(dest => dest.WorkshopsCount, opt => opt.Ignore());

        CreateMap<CreateProviderAdminDto, CreateProviderAdminRequest>()
            .ForMember(dest => dest.RequestId, opt => opt.Ignore())
            .ForMember(c => c.CreatingTime, m => m.MapFrom(c => Timestamp.FromDateTimeOffset(c.CreatingTime)))
            .ForMember(c => c.ProviderId, m => m.MapFrom(c => c.ProviderId.ToString()))
            .ForMember(c => c.ManagedWorkshopIds, m => m.MapFrom((dto, entity) =>
            {
                var managedWorkshopIds = new List<string>();

                foreach (var item in dto.ManagedWorkshopIds)
                {
                    managedWorkshopIds.Add(item.ToString());
                }

                return managedWorkshopIds;
            }));

        CreateMap<CreateProviderAdminReply, CreateProviderAdminDto>()
            .ForMember(c => c.CreatingTime, m => m.MapFrom(c => c.CreatingTime.ToDateTimeOffset()))
            .ForMember(c => c.ProviderId, m => m.MapFrom(c => Guid.Parse(c.ProviderId)))
            .ForMember(c => c.ManagedWorkshopIds, opt => opt.MapFrom((dto, entity) =>
            {
                var managedWorkshopIds = new List<Guid>();

                foreach (var item in dto.ManagedWorkshopIds)
                {
                    managedWorkshopIds.Add(Guid.Parse(item));
                }

                return managedWorkshopIds;
            }));

        CreateMap<User, ShortUserDto>()
            .ForMember(dest => dest.Gender, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore());

        CreateMap<ShortUserDto, User>()
            .ForMember(dest => dest.IsRegistered, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.CreatingTime, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.IsDerived, opt => opt.Ignore())
            .ForMember(dest => dest.MustChangePassword, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());

        CreateMap<Parent, ShortUserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.User.MiddleName))
            .ForMember(dest => dest.IsRegistered, opt => opt.MapFrom(src => src.User.IsRegistered))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

        CreateMap<BaseUserDto, User>()
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
            .ForMember(dest => dest.MustChangePassword, m => m.Ignore());

        CreateMap<InstitutionAdmin, MinistryAdminDto>()
            .ForMember(dest => dest.InstitutionTitle, opt => opt.MapFrom(src => src.Institution.Title))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.User.MiddleName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(
                dest => dest.AccountStatus,
                opt => opt.MapFrom(src =>
                    src.User.IsBlocked
                        ? AccountStatus.Blocked
                        : src.User.LastLogin == DateTimeOffset.MinValue
                            ? AccountStatus.NeverLogged
                            : AccountStatus.Accepted));

        CreateMap<RegionAdmin, RegionAdminDto>()
            .ForMember(dest => dest.InstitutionTitle, opt => opt.MapFrom(src => src.Institution.Title))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.User.MiddleName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.CATOTTGCategory, opt => opt.MapFrom(src => src.CATOTTG.Category))
            .ForMember(dest => dest.CATOTTGName, opt => opt.MapFrom(src => src.CATOTTG.Name))
            .ForMember(
                dest => dest.AccountStatus,
                opt => opt.MapFrom(src =>
                    src.User.IsBlocked
                        ? AccountStatus.Blocked
                        : src.User.LastLogin == DateTimeOffset.MinValue
                            ? AccountStatus.NeverLogged
                            : AccountStatus.Accepted));

        CreateMap<ProviderChangesLogRequest, ChangesLogFilter>()
            .ForMember(dest => dest.EntityType, opt => opt.Ignore())
            .ForMember(dest => dest.From, opt => opt.Ignore())
            .ForMember(dest => dest.Size, opt => opt.MapFrom(o => default(int)))
            .AfterMap((src, dest) => dest.EntityType = "Provider");

        CreateMap<ApplicationChangesLogRequest, ChangesLogFilter>()
            .ForMember(dest => dest.EntityType, opt => opt.Ignore())
            .ForMember(dest => dest.From, opt => opt.Ignore())
            .ForMember(dest => dest.Size, opt => opt.MapFrom(o => default(int)))
            .AfterMap((src, dest) => dest.EntityType = "Application");

        CreateMap<AchievementType, AchievementTypeDto>();

        CreateMap<Achievement, AchievementDto>();

        CreateMap<AchievementDto, Achievement>()
            .ForMember(dest => dest.Workshop, opt => opt.Ignore())
            .ForMember(dest => dest.AchievementType, opt => opt.Ignore());

        CreateMap<AchievementTeacher, AchievementTeacherDto>();

        CreateMap<AchievementTeacherDto, AchievementTeacher>()
            .ForMember(dest => dest.Achievement, opt => opt.Ignore());

        CreateMap<AchievementCreateDTO, Achievement>()
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Workshop, opt => opt.Ignore())
            .ForMember(dest => dest.AchievementType, opt => opt.Ignore())
            .ForMember(dest => dest.Teachers, opt => opt.Ignore());

        CreateMap<ProviderAdmin, ProviderAdminProviderRelationDto>();

        CreateMap<ChatMessageWorkshop, ChatMessageWorkshopDto>().ReverseMap();
        CreateMap<ChatRoomWorkshop, ChatRoomWorkshopDto>();
        CreateMap<Workshop, WorkshopInfoForChatListDto>();
        CreateMap<ChatRoomWorkshopForChatList, ChatRoomWorkshopDtoWithLastMessage>();
        CreateMap<WorkshopInfoForChatList, WorkshopInfoForChatListDto>();

        CreateMap<ParentInfoForChatList, ParentDtoWithContactInfo>()
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore());

        CreateMap<ChatMessageInfoForChatList, ChatMessageWorkshopDto>();

        CreateMap<Favorite, FavoriteDto>().ReverseMap();

        CreateMap<ApplicationDto, ParentCard>()
            .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.Workshop.ProviderId))
            .ForMember(dest => dest.ProviderTitle, opt => opt.MapFrom(src => src.Workshop.ProviderTitle))
            .ForMember(dest => dest.ProviderOwnership, opt => opt.MapFrom(src => src.Workshop.ProviderOwnership))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Workshop.Rating))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Workshop.Title))
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
            .ForMember(dest => dest.ProviderLicenseStatus, opt =>
                opt.MapFrom(src => src.Workshop.ProviderLicenseStatus));

        CreateMap<Rating, RatingDto>()
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore());
        CreateMap<RatingDto, Rating>()
            .ForMember(dest => dest.Parent, opt => opt.Ignore());

        CreateMap<InstitutionStatus, InstitutionStatusDTO>().ReverseMap();

        CreateMap<PermissionsForRole, PermissionsForRoleDTO>()
            .ForMember(dest => dest.Permissions,
                opt => opt.MapFrom(c => c.PackedPermissions.UnpackPermissionsFromString()));
        CreateMap<PermissionsForRoleDTO, PermissionsForRole>()
            .ForMember(dest => dest.PackedPermissions,
                opt => opt.MapFrom(c => c.Permissions.PackPermissionsIntoString()));

        CreateMap<Child, ShortEntityDto>()
            .ForMember(dest => dest.Title,
                opt => opt.MapFrom(src => src.LastName + " " + src.FirstName + " " + src.MiddleName));

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
                opt => opt.MapFrom(src => src.Category == CodeficatorCategory.CityDistrict.Name ? src.Name : null));

        CreateMap<CATOTTG, AllAddressPartsDto>()
            .IncludeBase<CATOTTG, CodeficatorAddressDto>()
            .ForMember(
                dest => dest.AddressParts,
                opt => opt.MapFrom(src => src));

        CreateMap<Provider, ProviderStatusDto>()
            .ForMember(
                dest => dest.ProviderId,
                opt =>
                    opt.MapFrom(src => src.Id))
            .ForMember(
                dest => dest.Status,
                opt =>
                    opt.MapFrom(src => src.Status))
            .ForMember(
                dest => dest.StatusReason,
                opt =>
                    opt.MapFrom(src => String.Empty));

        CreateMap<WorkshopFilter, WorkshopBySettlementsFilter>()
            .ForMember(dest => dest.SettlementsIds, opt => opt.Ignore());

        CreateMap<OwnershipTypeDto, OwnershipType>()
            .ConvertUsingEnumMapping(opt => opt
                    .MapValue(OwnershipTypeDto.Common, OwnershipType.Common)
                    .MapValue(OwnershipTypeDto.State, OwnershipType.State))
            .ReverseMap(opt => opt
                .MapValue(OwnershipType.Common, OwnershipTypeDto.Common)
                .MapValue(OwnershipType.State, OwnershipTypeDto.State)
                // TODO: This acts as a default value, technically should not happen because you can't create
                // TODO: private provider
                .MapValue(OwnershipType.Private, OwnershipTypeDto.State));
    }
}