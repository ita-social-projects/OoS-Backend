using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using OutOfSchool.Common.Models;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;
using OutOfSchool.WebApi.Models.BlockedProviderParent;
using OutOfSchool.WebApi.Models.Changes;
using OutOfSchool.WebApi.Models.Codeficator;
using OutOfSchool.WebApi.Models.Notifications;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.SubordinationStructure;
using OutOfSchool.WebApi.Models.Teachers;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Util.CustomComparers;

namespace OutOfSchool.WebApi.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        const char SEPARATOR = '¤';
        CreateMap<WorkshopDTO, Workshop>()
            .ForMember(
                dest => dest.Keywords,
                opt => opt.MapFrom(src => string.Join(SEPARATOR, src.Keywords.Distinct())))
            .ForMember(dest => dest.Direction, opt => opt.Ignore())
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
            .ForMember(dest => dest.Class, opt => opt.Ignore())
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
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.Title))
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)))
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.MapFrom(src => src.InstitutionHierarchy.Title))
            .ForMember(dest => dest.Directions, opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore());

        CreateMap<WorkshopDescriptionItem, WorkshopDescriptionItemDto>().ReverseMap();

        CreateMap<Address, AddressDto>().ReverseMap();

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

        CreateMap<Provider, ProviderDto>()
            .ForMember(dest => dest.ActualAddress, opt => opt.MapFrom(src => src.ActualAddress))
            .ForMember(dest => dest.LegalAddress, opt => opt.MapFrom(src => src.LegalAddress))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution))
            .ForMember(dest => dest.EdrpouIpn, opt => opt.MapFrom(src => src.EdrpouIpn.ToString()))
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ForMember(dest => dest.ImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)));

        CreateMap<ProviderDto, Provider>()
            .ForMember(dest => dest.EdrpouIpn, opt => opt.MapFrom(src => long.Parse(src.EdrpouIpn)))
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

        CreateMap<Application, ApplicationDto>().ReverseMap();

        CreateMap<WorkshopCard, Workshop>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Phone, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.Website, opt => opt.Ignore())
            .ForMember(dest => dest.Facebook, opt => opt.Ignore())
            .ForMember(dest => dest.Instagram, opt => opt.Ignore())
            .ForMember(dest => dest.Direction, opt => opt.Ignore())
            .ForMember(dest => dest.WorkshopDescriptionItems, opt => opt.Ignore())
            .ForMember(dest => dest.DisabilityOptionsDesc, opt => opt.Ignore())
            .ForMember(dest => dest.Keywords, opt => opt.Ignore())
            .ForMember(dest => dest.DepartmentId, opt => opt.Ignore())
            .ForMember(dest => dest.ClassId, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.Ignore())
            .ForMember(dest => dest.Class, opt => opt.Ignore())
            .ForMember(dest => dest.Teachers, opt => opt.Ignore())
            .ForMember(dest => dest.Applications, opt => opt.Ignore())
            .ForMember(dest => dest.DateTimeRanges, opt => opt.Ignore())
            .ForMember(dest => dest.ChatRooms, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderAdmins, opt => opt.Ignore());

        CreateMap<Workshop, WorkshopCard>()
            .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
            .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(s => s.CoverImageId))
            .ForMember(dest => dest.DirectionId, opt => opt.MapFrom(src => src.Direction.Id))
            .ForMember(dest => dest.DirectionsId, opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions.Select(x => x.Id)))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(dest => dest.Rating, opt => opt.Ignore());

        CreateMap<SocialGroup, SocialGroupDto>().ReverseMap();

        CreateMap<Child, ChildDto>().ReverseMap()
            .ForMember(c => c.Parent, m => m.Ignore());

        CreateMap<Parent, ParentDTO>().ReverseMap();

        CreateMap<Parent, ParentDtoWithContactInfo>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(s => s.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(s => s.User.PhoneNumber))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(s => s.User.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(s => s.User.MiddleName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(s => s.User.FirstName));

        CreateMap<CompanyInformationItem, CompanyInformationItemDto>().ReverseMap();
        CreateMap<CompanyInformation, CompanyInformationDto>().ReverseMap();

        CreateMap<InstitutionHierarchy, InstitutionHierarchyDto>().ReverseMap();
        CreateMap<Institution, InstitutionDto>().ReverseMap();
        CreateMap<InstitutionFieldDescription, InstitutionFieldDescriptionDto>().ReverseMap();

        CreateMap<Codeficator, CodeficatorDto>();
        CreateMap<Codeficator, CodeficatorWithParentDto>();

        CreateMap<Notification, NotificationDto>().ReverseMap()
            .ForMember(n => n.Id, n => n.Ignore());

        CreateMap<ElasticsearchSyncRecord, ElasticsearchSyncRecordDto>().ReverseMap();

        CreateMap<Address, AddressES>()
            .ForMember(
                dest => dest.Point,
                opt => opt.MapFrom(gl => new Nest.GeoLocation(gl.Latitude, gl.Longitude)));

        CreateMap<DateTimeRange, DateTimeRangeES>()
            .ForMember(
                dest => dest.Workdays,
                opt => opt.MapFrom(dtr => string.Join(" ", dtr.Workdays.ToDaysBitMaskEnumerable())));

        CreateMap<Teacher, TeacherES>()
            .ForMember(dest => dest.Image, opt => opt.Ignore());

        CreateMap<Direction, DirectionES>();

        CreateMap<Workshop, WorkshopES>()
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.Direction, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchy, opt => opt.MapFrom(src => src.InstitutionHierarchy.Title))
            .ForMember(dest => dest.Directions, opt => opt.MapFrom(src => src.InstitutionHierarchy.Directions))
            .ForMember(dest => dest.InstitutionId, opt => opt.MapFrom(src => src.InstitutionHierarchy.InstitutionId))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.InstitutionHierarchy.Institution.Title))
            .ForMember(
                dest => dest.Description,
                opt =>
                    opt.MapFrom(src =>
                        src.WorkshopDescriptionItems
                            .Aggregate(string.Empty, (accumulator, wdi) =>
                                $"{accumulator}{wdi.SectionName}{SEPARATOR}{wdi.Description}{SEPARATOR}")));

#warning The next mapping is here to test UI Admin features. Will be removed or refactored
        CreateMap<ShortUserDto, AdminDto>();

        CreateMap<User, ProviderAdminDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(c => c.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(c => c.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(c => c.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(c => c.MiddleName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(c => c.PhoneNumber))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(c => c.Email))
            .ForMember(dest => dest.IsDeputy, opt => opt.Ignore())
            .ForMember(dest => dest.AccountStatus, m => m.Ignore());

        CreateMap<ClassDto, Class>()
            .ForMember(dest => dest.Department, opt => opt.Ignore());

        CreateMap<Class, ClassDto>();

        CreateMap<DepartmentDto, Department>()
            .ForMember(dest => dest.Direction, opt => opt.Ignore())
            .ForMember(dest => dest.Classes, opt => opt.Ignore());

        CreateMap<Department, DepartmentDto>();

        CreateMap<DirectionDto, Direction>()
            .ForMember(dest => dest.Departments, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionHierarchies, opt => opt.Ignore());

        CreateMap<Direction, DirectionDto>();

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

        CreateMap<User, ShortUserDto>();

        CreateMap<ProviderChangesLogRequest, ChangesLogFilter>()
            .ForMember(dest => dest.EntityType, opt => opt.Ignore())
            .AfterMap((src, dest) => dest.EntityType = "Provider");

        CreateMap<ApplicationChangesLogRequest, ChangesLogFilter>()
            .ForMember(dest => dest.EntityType, opt => opt.Ignore())
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
    }
}