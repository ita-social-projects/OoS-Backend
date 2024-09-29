using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateEmployeeDto, User>()
            .Apply(MapEmailAndPhone);

        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.ManagedWorkshops, opt => opt.Ignore());

        CreateMap<CreateEmployeeDto, CreateProviderAdminReply>()
            .ForMember(c => c.CreatingTime, m => m.MapFrom(c => Timestamp.FromDateTimeOffset(c.CreatingTime)))
            .ForMember(c => c.ProviderId, m => m.MapFrom(c => c.ProviderId.ToString()))
            .ForMember(c => c.ManagedWorkshopIds, m => m.MapFrom(src => src.ManagedWorkshopIds.Select(id => id.ToString()).ToList()));

        CreateMap<CreateProviderAdminRequest, CreateEmployeeDto>()
            .ForMember(c => c.CreatingTime, m => m.MapFrom(c => c.CreatingTime.ToDateTimeOffset()))
            .ForMember(c => c.ProviderId, m => m.MapFrom(c => Guid.Parse(c.ProviderId)))
            .ForMember(c => c.ManagedWorkshopIds, opt => opt.MapFrom(src => src.ManagedWorkshopIds.Select(Guid.Parse).ToList()));

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