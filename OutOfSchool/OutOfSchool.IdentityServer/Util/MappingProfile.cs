using System;
using System.Collections.Generic;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateProviderAdminDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => Constants.PhonePrefix + src.PhoneNumber.Right(Constants.PhoneShortLength)));

        CreateMap<CreateProviderAdminDto, ProviderAdmin>()
            .ForMember(dest => dest.ManagedWorkshops, opt => opt.Ignore());

        CreateMap<CreateProviderAdminDto, CreateProviderAdminReply>()
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

        CreateMap<CreateProviderAdminRequest, CreateProviderAdminDto>()
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

        CreateMap<MinistryAdminBaseDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => Constants.PhonePrefix + src.PhoneNumber.Right(Constants.PhoneShortLength)));

        CreateMap<MinistryAdminBaseDto, InstitutionAdmin>();

        CreateMap<RegionAdminBaseDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => Constants.PhonePrefix + src.PhoneNumber.Right(Constants.PhoneShortLength)));

        CreateMap<RegionAdminBaseDto, RegionAdmin>();
    }
}