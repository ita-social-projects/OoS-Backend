using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Teachers;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Util.CustomComparers;

namespace OutOfSchool.WebApi.Util
{
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
                .ForMember(dest => dest.Teachers, opt => opt.MapFrom((dto, entity, dest, ctx) =>
                {
                    var dtoTeachers = ctx.Mapper.Map<List<Teacher>>(dto.Teachers);
                    return WorkshopTeachersMapperFunction(dtoTeachers, dest);
                }))
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.CoverImageId, opt => opt.Ignore());

            CreateMap<WorkshopCreationDto, Workshop>()
                .IncludeBase<WorkshopDTO, Workshop>()
                .ForMember(dest => dest.Teachers, opt => opt.MapFrom((dto, entity, dest, ctx) =>
                {
                    var dtoTeachers = ctx.Mapper.Map<List<Teacher>>(dto.Teachers);
                    return WorkshopTeachersMapperFunction(dtoTeachers, dest);
                })); // duplicate for Teachers here because WorkshopCreationDto hides WorkshopDTO.Teachers

            CreateMap<WorkshopUpdateDto, Workshop>()
                .IncludeBase<WorkshopDTO, Workshop>()
                .ForMember(dest => dest.Teachers, opt => opt.Ignore());

            CreateMap<Workshop, WorkshopDTO>()
                .ForMember(
                    dest => dest.Keywords,
                    opt => opt.MapFrom(src => src.Keywords.Split(SEPARATOR, StringSplitOptions.None)))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.Title))
                .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => src.Images.Select(x => x.ExternalStorageId)));
            CreateMap<Address, AddressDto>().ReverseMap();

            CreateMap<Provider, ProviderDto>()
                 .ForMember(dest => dest.ActualAddress, opt => opt.MapFrom(src => src.ActualAddress))
                 .ForMember(dest => dest.LegalAddress, opt => opt.MapFrom(src => src.LegalAddress))
                 .ForMember(dest => dest.EdrpouIpn, opt => opt.MapFrom(src => src.EdrpouIpn.ToString()))
                 .ForMember(dest => dest.Rating, opt => opt.Ignore())
                 .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore());

            CreateMap<ProviderDto, Provider>()
                 .ForMember(dest => dest.EdrpouIpn, opt => opt.MapFrom(src => long.Parse(src.EdrpouIpn)))
                 .ForMember(dest => dest.Workshops, opt => opt.Ignore())
                 .ForMember(dest => dest.User, opt => opt.Ignore())
                 .ForMember(dest => dest.InstitutionStatus, opt => opt.Ignore());

            CreateMap<TeacherDTO, Teacher>()
                .ForMember(dest => dest.AvatarImageId, opt => opt.Ignore())
                .ForMember(dest => dest.WorkshopId, opt => opt.Ignore());
            CreateMap<TeacherCreationDto, Teacher>()
                .IncludeBase<TeacherDTO, Teacher>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(src => src.WorkshopId));
            CreateMap<TeacherUpdateDto, Teacher>()
                .IncludeBase<TeacherDTO, Teacher>()
                .ForMember(dest => dest.Id, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.WorkshopId, opt => opt.UseDestinationValue());
            CreateMap<TeacherUpdateDto, TeacherCreationDto>();
            CreateMap<Teacher, TeacherDTO>();

            CreateMap<DateTimeRange, DateTimeRangeDto>()
                .ForMember(dtr => dtr.Workdays, cfg => cfg.MapFrom(dtr => dtr.Workdays.ToDaysBitMaskEnumerable().ToList()));
            CreateMap<DateTimeRangeDto, DateTimeRange>()
                .ForMember(dtr => dtr.Workdays, cfg => cfg.MapFrom(dtr => dtr.Workdays.ToDaysBitMask()));
            CreateMap<Application, ApplicationDto>().ReverseMap();
            CreateMap<WorkshopCard, Workshop>()
                .ForMember(dest => dest.Direction, opt => opt.Ignore());
            CreateMap<Workshop, WorkshopCard>()
                .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(s => s.CoverImageId))
                .ForMember(dest => dest.DirectionId, opt => opt.MapFrom(src => src.Direction.Id));
            CreateMap<Child, ChildDto>().ReverseMap()
                .ForMember(c => c.Parent, m => m.Ignore());
            CreateMap<Parent, ParentDTO>().ReverseMap();

            CreateMap<AboutPortalItem, AboutPortalItemDto>().ReverseMap();
            CreateMap<AboutPortal, AboutPortalDto>()
                .ForMember(dest => dest.AboutPortalItems, opt => opt.MapFrom((dto, entity, dest, ctx) =>
                {
                    var dtoItems = ctx.Mapper.Map<List<AboutPortalItem>>(dto.AboutPortalItems);
                    return dtoItems;
                }));
            CreateMap<AboutPortalDto, AboutPortal>();

            CreateMap<SupportInformation, SupportInformationDto>().ReverseMap()
                .ForMember(c => c.Id, m => m.Ignore());

            CreateMap<ElasticsearchSyncRecord, ElasticsearchSyncRecordDto>().ReverseMap();

            CreateMap<Address, AddressES>()
                .ForMember(
                    dest => dest.Point,
                    opt => opt.MapFrom(gl => new Nest.GeoLocation(gl.Latitude, gl.Longitude)));

            CreateMap<DateTimeRange, DateTimeRangeES>()
                .ForMember(
                    dest => dest.Workdays,
                    opt => opt.MapFrom(dtr => string.Join(" ", dtr.Workdays.ToDaysBitMaskEnumerable())));

            CreateMap<Teacher, TeacherES>();

            CreateMap<Workshop, WorkshopES>()
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.Direction, opt => opt.Ignore());
            #warning The next mapping is here to test UI Admin features. Will be removed or refactored
            CreateMap<ShortUserDto, AdminDto>();
        }

        private static List<Teacher> WorkshopTeachersMapperFunction(List<Teacher> dtoTeachers, List<Teacher> dest)
        {
            if (dest is { } && dest.Any())
            {
                var dtoTeachersHs = new HashSet<Teacher>(dtoTeachers, new TeacherComparerWithoutFK());
                foreach (var destTeacher in dest.Where(destTeacher => dtoTeachersHs.Remove(destTeacher)))
                {
                    dtoTeachersHs.Add(destTeacher);
                }

                return dtoTeachersHs.ToList();
            }

            return dtoTeachers;
        }
    }
}
