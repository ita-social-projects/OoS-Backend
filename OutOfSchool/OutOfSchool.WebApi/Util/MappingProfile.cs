using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util.CustomComparers;

namespace OutOfSchool.WebApi.Util
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            const char SEPARATOR = '¤';
            CreateMap<WorkshopDTO, Workshop>()
                .ForMember(dest => dest.Keywords,
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
                }))
                ;

            CreateMap<Workshop, WorkshopDTO>()
                .ForMember(
                    dest => dest.Keywords,
                    opt => opt.MapFrom(src => src.Keywords.Split(SEPARATOR, StringSplitOptions.None)))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.Title));
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<Provider, ProviderDto>().ReverseMap();
            CreateMap<Teacher, TeacherDTO>().ReverseMap();
            CreateMap<DateTimeRange, DateTimeRangeDto>()
                .ForMember(dtr => dtr.Workdays, cfg => cfg.MapFrom(dtr => dtr.Workdays.ToDaysBitMaskEnumerable().ToList()));
            CreateMap<DateTimeRangeDto, DateTimeRange>()
                .ForMember(dtr => dtr.Workdays, cfg => cfg.MapFrom(dtr => dtr.Workdays.ToDaysBitMask()));
            CreateMap<Application, ApplicationDto>().ReverseMap();
            CreateMap<WorkshopCard, Workshop>()
                .ForMember(dest => dest.Direction, opt => opt.Ignore());
            CreateMap<Workshop, WorkshopCard>()
                .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(s => s.Logo))
                .ForMember(dest => dest.DirectionId, opt => opt.MapFrom(src => src.Direction.Title));
            CreateMap<Child, ChildDto>().ReverseMap()
                .ForMember(c => c.Parent, m => m.Ignore());
            CreateMap<Parent, ParentDTO>().ReverseMap();
        }
    }
}