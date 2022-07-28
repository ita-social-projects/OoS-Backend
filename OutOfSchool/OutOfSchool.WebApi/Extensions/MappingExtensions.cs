using System;
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