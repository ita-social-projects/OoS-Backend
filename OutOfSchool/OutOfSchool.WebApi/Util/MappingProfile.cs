using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util.CustomComparers;

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
                        var dtoTimeRangesHs =
                            new HashSet<DateTimeRange>(dateTimeRanges, new DateTimeRangeComparerWithoutFK());
                        foreach (var destDateTimeRange in dest)
                        {
                            if (dtoTimeRangesHs.TryGetValue(destDateTimeRange, out var dtoTimeRange) &&
                                dtoTimeRangesHs.Remove(destDateTimeRange))
                            {
                                destDateTimeRange.Workdays = UseSameRefsForTheSameWorkdays(
                                    dtoTimeRange.Workdays, destDateTimeRange.Workdays).ToList();

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
                    opt => opt.MapFrom(src => src.Keywords.Split('¤', StringSplitOptions.None)))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.Title));
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<Provider, ProviderDto>().ReverseMap();
            CreateMap<Teacher, TeacherDTO>().ReverseMap();
            CreateMap<DateTimeRange, DateTimeRangeDto>().ReverseMap();
            CreateMap<Workday, WorkdayDto>().ReverseMap();
        }

        private static HashSet<Workday> UseSameRefsForTheSameWorkdays(
            IEnumerable<Workday> baseWorkdays, IEnumerable<Workday> workdaysToBeReplaced)
        {
            var baseWorkdaysHs = new HashSet<Workday>(baseWorkdays, new WorkdayComparerWithoutFK());
            foreach (var destWd in workdaysToBeReplaced.Where(destWd => baseWorkdaysHs.Remove(destWd)))
            {
                baseWorkdaysHs.Add(destWd);
            }

            return baseWorkdaysHs;
        }
    }
}