using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Util
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<WorkshopDTO, Workshop>()
                .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => string.Join('¤', src.Keywords.Distinct())))
                .ForMember(dest => dest.Direction, opt => opt.Ignore())
                .ForMember(dest => dest.DateTimeRanges, opt => opt.MapFrom((dto, entity, dest, ctx) =>
                {
                    var dateTimeRanges = ctx.Mapper.Map<List<DateTimeRange>>(dto.DateTimeRanges);
                    if (dest is { } && dest.Any())
                    {
                        var dtoTimeRangesHs = new HashSet<DateTimeRange>(dateTimeRanges);
                        foreach (var destDateTimeRange in dest)
                        {
                            if (dtoTimeRangesHs.TryGetValue(destDateTimeRange, out var dtoTimeRange) &&
                                dtoTimeRangesHs.Remove(destDateTimeRange))
                            {
                                if (!destDateTimeRange.Workdays.SequenceEqual(dtoTimeRange.Workdays))
                                {
                                    var dtoWorkdayHs = new HashSet<Workday>(dtoTimeRange.Workdays);
                                    foreach (var destWd in destDateTimeRange.Workdays.Where(destWd => dtoWorkdayHs.Remove(destWd)))
                                    {
                                        dtoWorkdayHs.Add(destWd);
                                    }

                                    destDateTimeRange.Workdays = dtoWorkdayHs.ToList();
                                }

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
                        var dtoTeachersHs = new HashSet<Teacher>(dtoTeachers);
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
                    opt => opt.MapFrom(src => src.Keywords.Split('¤', StringSplitOptions.None)))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.Title));
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<Provider, ProviderDto>().ReverseMap();
            CreateMap<Teacher, TeacherDTO>().ReverseMap();
            CreateMap<DateTimeRange, DateTimeRangeDto>().ReverseMap();
            CreateMap<Workday, WorkdayDto>().ReverseMap();
        }
    }
}