using AutoMapper;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<MinistryAdminBaseDto, User>()
            .Apply(MapEmailAndPhone);

        CreateMap<MinistryAdminBaseDto, InstitutionAdmin>();

        CreateMap<RegionAdminBaseDto, User>()
            .Apply(MapEmailAndPhone);

        CreateMap<RegionAdminBaseDto, RegionAdmin>();

        CreateMap<AreaAdminBaseDto, User>()
            .Apply(MapEmailAndPhone);

        CreateMap<AreaAdminBaseDto, AreaAdmin>();
    }

    private IMappingExpression<TSource, User> MapEmailAndPhone<TSource>(IMappingExpression<TSource, User> mappings)
        where TSource : AdminBaseDto
        => mappings
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
}