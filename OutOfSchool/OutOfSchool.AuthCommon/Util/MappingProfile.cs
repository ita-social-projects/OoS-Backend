using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.AuthCommon.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateProviderAdminDto, User>()
            .Apply(MapEmailAndPhone);

        CreateMap<CreateProviderAdminDto, ProviderAdmin>()
            .ForMember(dest => dest.ManagedWorkshops, opt => opt.Ignore());

        CreateMap<CreateProviderAdminDto, CreateProviderAdminReply>()
            .ForMember(c => c.CreatingTime, m => m.MapFrom(c => Timestamp.FromDateTimeOffset(c.CreatingTime)))
            .ForMember(c => c.ProviderId, m => m.MapFrom(c => c.ProviderId.ToString()))
            .ForMember(c => c.ManagedWorkshopIds, m => m.MapFrom(src => src.ManagedWorkshopIds.Select(id => id.ToString()).ToList()));

        CreateMap<CreateProviderAdminRequest, CreateProviderAdminDto>()
            .ForMember(c => c.CreatingTime, m => m.MapFrom(c => c.CreatingTime.ToDateTimeOffset()))
            .ForMember(c => c.ProviderId, m => m.MapFrom(c => Guid.Parse(c.ProviderId)))
            .ForMember(c => c.ManagedWorkshopIds, opt => opt.MapFrom(src => src.ManagedWorkshopIds.Select(Guid.Parse).ToList()));

        CreateMap<Provider, ProviderDto>()
            .ForMember(dest => dest.BlockReason, opt => opt.MapFrom(scr => scr.BlockReason))
            .ForMember(dest => dest.Ownership, opt => opt.MapFrom(scr => scr.Ownership))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(scr => scr.IsBlocked))
            .ForMember(dest => dest.Institution, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionType, opt => opt.Ignore())
            .ForMember(dest => dest.InstitutionId, opt => opt.Ignore())
            .ForMember(dest => dest.FullTitle, opt => opt.Ignore())
            .ForMember(dest => dest.ShortTitle, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.EdrpouIpn, opt => opt.Ignore())
            .ForMember(dest => dest.Director, opt => opt.Ignore())
            .ForMember(dest => dest.DirectorDateOfBirth, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Founder, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.BlockPhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ActualAddress, opt => opt.Ignore())
            .ForMember(dest => dest.LegalAddress, opt => opt.Ignore());

        CreateMap<Address, AddressDto>();

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
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => Constants.PhonePrefix + src.PhoneNumber.Right(Constants.PhoneShortLength)))
        ;
}