using AutoMapper;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateProviderAdminDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<CreateProviderAdminDto, ProviderAdmin>()
                .ForMember(dest => dest.ManagedWorkshops, opt => opt.Ignore());
        }
    }
}
