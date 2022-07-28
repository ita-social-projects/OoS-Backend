﻿using System;
using AutoMapper;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Extensions;

public static class MappingExtensions
{
    #region ToModel

    public static ProviderDto ToModel(this Provider provider)
    {
        return Mapper<Provider, ProviderDto>(provider, cfg =>
        {
            cfg.CreateMap<Address, AddressDto>();
            cfg.CreateMap<ProviderSectionItem, ProviderSectionItemDto>()
                .ForMember(
                    dest => dest.SectionName, opt =>
                        opt.MapFrom(src => src.Name));
            cfg.CreateMap<Provider, ProviderDto>()
                .ForMember(dest => dest.ActualAddress, opt => opt.MapFrom(src => src.ActualAddress))
                .ForMember(dest => dest.LegalAddress, opt => opt.MapFrom(src => src.LegalAddress))
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore());
        });
    }

    public static RatingDto ToModel(this Rating rating)
    {
        return Mapper<Rating, RatingDto>(rating, cfg => { cfg.CreateMap<Rating, RatingDto>(); });
    }

    public static ShortUserDto ToModel(this User user)
    {
        return Mapper<User, ShortUserDto>(user, cfg =>
        {
            cfg.CreateMap<User, ShortUserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(c => c.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
                .ForMember(dest => dest.IsRegistered, opt => opt.MapFrom(src => src.IsRegistered))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
        });
    }

    public static SocialGroupDto ToModel(this SocialGroup group)
    {
        return Mapper<SocialGroup, SocialGroupDto>(group, cfg => { cfg.CreateMap<SocialGroup, SocialGroupDto>(); });
    }

    public static InstitutionStatusDTO ToModel(this InstitutionStatus status)
    {
        return Mapper<InstitutionStatus, InstitutionStatusDTO>(status, cfg => { cfg.CreateMap<InstitutionStatus, InstitutionStatusDTO>(); });
    }

    public static PermissionsForRoleDTO ToModel(this PermissionsForRole permissionsForRole)
    {
        return Mapper<PermissionsForRole, PermissionsForRoleDTO>(permissionsForRole, cfg =>
        {
            cfg.CreateMap<PermissionsForRole, PermissionsForRoleDTO>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(c => c.PackedPermissions.UnpackPermissionsFromString()));
        });
    }

    public static TeacherDTO ToModel(this Teacher teacher)
    {
        return Mapper<Teacher, TeacherDTO>(teacher, cfg =>
        {
            cfg.CreateMap<Teacher, TeacherDTO>();
            cfg.CreateMap<Workshop, WorkshopDTO>();
        });
    }

    public static WorkshopDTO ToModel(this Workshop workshop)
    {
        return Mapper<Workshop, WorkshopDTO>(workshop, cfg =>
        {
            cfg.CreateMap<Workshop, WorkshopDTO>()
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.Title));
            cfg.CreateMap<Address, AddressDto>();
            cfg.CreateMap<Provider, ProviderDto>();
            cfg.CreateMap<Teacher, TeacherDTO>();
            cfg.CreateMap<WorkshopDescriptionItem, WorkshopDescriptionItemDto>();
        });
    }

    #endregion

    #region ToDomain

    public static Provider ToDomain(this ProviderDto providerDto)
    {
        return Mapper<ProviderDto, Provider>(providerDto, cfg =>
        {
            cfg.CreateMap<AddressDto, Address>();
            cfg.CreateMap<ProviderSectionItemDto, ProviderSectionItem>()
                .ForMember(
                    dest => dest.Name, opt =>
                        opt.MapFrom(src => src.SectionName));
            cfg.CreateMap<ProviderDto, Provider>()
                .ForMember(dest => dest.Workshops, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.InstitutionStatus, opt => opt.Ignore());
        });
    }

    public static Rating ToDomain(this RatingDto ratingDto)
    {
        return Mapper<RatingDto, Rating>(ratingDto, cfg => { cfg.CreateMap<RatingDto, Rating>(); });
    }

    public static SocialGroup ToDomain(this SocialGroupDto groupDto)
    {
        return Mapper<SocialGroupDto, SocialGroup>(groupDto, cfg => { cfg.CreateMap<SocialGroupDto, SocialGroup>(); });
    }

    public static InstitutionStatus ToDomain(this InstitutionStatusDTO statusDTO)
    {
        return Mapper<InstitutionStatusDTO, InstitutionStatus>(statusDTO, cfg => { cfg.CreateMap<InstitutionStatusDTO, InstitutionStatus>(); });
    }

    public static PermissionsForRole ToDomain(this PermissionsForRoleDTO permissionsDTO)
    {
        return Mapper<PermissionsForRoleDTO, PermissionsForRole>(permissionsDTO, cfg =>
        {
            cfg.CreateMap<PermissionsForRoleDTO, PermissionsForRole>()
                .ForMember(dest => dest.PackedPermissions, opt => opt.MapFrom(c => c.Permissions.PackPermissionsIntoString()));
        });
    }

    public static User ToDomain(this ShortUserDto shortUserDto, User user)
    {
        return Mapper<ShortUserDto, User>(shortUserDto, cfg =>
        {
            cfg.CreateMap<ShortUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(c => c.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.IsRegistered, opt => opt.MapFrom(src => user.IsRegistered))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => user.Role))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => user.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => user.UserName))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => user.LastLogin))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => user.NormalizedEmail))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => user.NormalizedUserName))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => user.EmailConfirmed))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => user.PasswordHash))
                .ForMember(dest => dest.SecurityStamp, opt => opt.MapFrom(src => user.SecurityStamp))
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.MapFrom(src => user.ConcurrencyStamp))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => user.PhoneNumberConfirmed))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => user.TwoFactorEnabled))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => user.LockoutEnabled))
                .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => user.LockoutEnd))
                .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => user.AccessFailedCount));
        });
    }

    #endregion

    #region ToCard

    public static WorkshopCard ToCardDto(this WorkshopDTO workshopDTO)
    {
        return Mapper<WorkshopDTO, WorkshopCard>(workshopDTO, cfg =>
        {
            cfg.CreateMap<WorkshopDTO, WorkshopCard>()
                .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.CoverImageId, opt => opt.MapFrom(s => s.CoverImageId));
        });
    }

    #endregion

    private static TDestination Mapper<TSource, TDestination>(
        this TSource source,
        Action<IMapperConfigurationExpression> configure)
    {
        var config = new MapperConfiguration(configure);
        var mapper = config.CreateMapper();
        var destination = mapper.Map<TDestination>(source);
        return destination;
    }
}