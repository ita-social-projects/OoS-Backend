#nullable enable

using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? user;
    private readonly IParentRepository parentRepository;
    private readonly IEntityRepositorySoftDeleted<Guid, Child> childRepository;
    private readonly IProviderRepository providerRepository;
    private readonly IProviderAdminRepository providerAdminRepository;
    private readonly ILogger<CurrentUserService> logger;
    private readonly ICacheService cache;
    private readonly AppDefaultsConfig options;
    private readonly IMapper mapper;

    public CurrentUserService(
        ClaimsPrincipal? user,
        IProviderRepository providerRepository,
        IProviderAdminRepository providerAdminRepository,
        IParentRepository parentRepository,
        IEntityRepositorySoftDeleted<Guid, Child> childRepository,
        ILogger<CurrentUserService> logger,
        ICacheService cache,
        IOptions<AppDefaultsConfig> options,
        IMapper mapper)
    {
        this.user = user;
        this.providerRepository = providerRepository;
        this.providerAdminRepository = providerAdminRepository;
        this.parentRepository = parentRepository;
        this.childRepository = childRepository;
        this.logger = logger;
        this.cache = cache;
        this.options = options.Value;
        this.mapper = mapper;
    }

    public string UserId => user?.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? string.Empty;

    public string UserRole => GettingUserProperties.GetUserRole(user);

    public string UserSubRole => GettingUserProperties.GetUserSubrole(user);

    public bool IsInRole(Role role) => role switch
    {
        Role.Provider => user?.IsInRole("provider") ?? false,
        Role.Parent => user?.IsInRole("parent") ?? false,
        Role.TechAdmin => user?.IsInRole("techadmin") ?? false,
        Role.MinistryAdmin => user?.IsInRole("ministryadmin") ?? false,
        Role.RegionAdmin => user?.IsInRole("regionadmin") ?? false,
        Role.AreaAdmin => user?.IsInRole("areaadmin") ?? false,
        _ => throw new NotImplementedException("Role not handled"),
    };

    public bool IsDeputyOrProviderAdmin() =>
        IsInRole(Role.Provider) &&
        (IsInSubRole(Subrole.ProviderDeputy) || IsInSubRole(Subrole.ProviderAdmin));

    public bool IsAdmin() => IsInRole(Role.TechAdmin) || IsInRole(Role.MinistryAdmin) || IsInRole(Role.RegionAdmin) || IsInRole(Role.AreaAdmin);

    public bool IsTechAdmin() => IsInRole(Role.TechAdmin);

    public bool IsMinistryAdmin() => IsInRole(Role.MinistryAdmin);

    public bool IsRegionAdmin() => IsInRole(Role.RegionAdmin);

    public bool IsAreaAdmin() => IsInRole(Role.AreaAdmin);

    public async Task UserHasRights(params IUserRights[] userTypes)
    {
        var userHasRights = false;
        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var parent = userTypes.OfType<ParentRights>().FirstOrDefault();
            var provider = userTypes.OfType<ProviderRights>().FirstOrDefault();
            var providerAdmin = userTypes.OfType<ProviderAdminRights>().FirstOrDefault();
            var providerAdminWorkshop = userTypes.OfType<ProviderAdminWorkshopRights>().FirstOrDefault();
            var providerDeputy = userTypes.OfType<ProviderDeputyRights>().FirstOrDefault();

            var result = await Task.WhenAll(
                new List<Task<bool>>
                    {
                        UserHasRights(parent),
                        UserHasRights(provider),
                        UserHasRights(providerAdmin),
                        UserHasRights(providerAdminWorkshop),
                        UserHasRights(providerDeputy),
                    }
                    .Select(Execute));
            userHasRights = result.Any(hasRight => hasRight);
        }
        else
        {
            if (options.AccessLogEnabled)
            {
                logger.LogWarning("Unauthenticated user tried accessing private data");
            }
        }

        if (!userHasRights)
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation");
        }
    }

    private async Task<bool> Execute(Task<bool> original)
    {
        try
        {
            return await original;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Checking rights failed");
            return false;
        }
    }

    private Task<bool> UserHasRights<T>(T? userType)
        where T : IUserRights
        => userType switch
        {
            ParentRights parent => ParentHasRights(parent.parentId, parent.childId),
            ProviderAdminRights providerAdmin => ProviderAdminHasRights(providerAdmin.providerAdminId),
            ProviderAdminWorkshopRights providerAdminWorkshop => ProviderAdminHasWorkshopRights(providerAdminWorkshop.providerId, providerAdminWorkshop.workshopId),
            ProviderRights provider => ProviderHasRights(provider.providerId),
            ProviderDeputyRights providerDeputy => ProviderDeputyHasRights(providerDeputy.providerId),
            null => Task.FromResult(false),
            _ => throw new NotImplementedException("Unknown user rights type"),
        };

    private async Task<bool> ParentHasRights(Guid parentId, Guid childId)
    {
        if (!IsInRole(Role.Parent))
        {
            return false;
        }

        var parent = await cache.GetOrAddAsync(
            $"Rights_{UserId}",
            async () =>
            {
                var parents = await parentRepository
                    .GetByFilter(p => p.UserId == UserId && p.Id == parentId);
                return parents?.Select(mapper.Map<ParentDTO>).FirstOrDefault();
            },
            TimeSpan.FromMinutes(5.0));

        // parentId == parent?.Id check is done in the filter,
        // so only need to check if the filter worked
        var result = parent is not null;

        if (result && childId != Guid.Empty)
        {
            var child = await cache.GetOrAddAsync(
                $"Rights_{UserId}_{childId}",
                async () =>
                {
                    var children = await childRepository
                        .GetByFilter(child => child.Id == childId && child.ParentId == parentId);
                    return children?.Select(mapper.Map<ChildDto>).FirstOrDefault();
                },
                TimeSpan.FromMinutes(5.0));

            // parentId == child?.ParentId check is done in the filter,
            // so only need to check if the filter worked
            result = child is not null;
        }

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access Parent ({ParentId}) data",
                UserId,
                parentId);
        }

        return result;
    }

    private async Task<bool> ProviderHasRights(Guid providerId)
    {
        if (!IsInRole(Role.Provider) || IsDeputyOrProviderAdmin())
        {
            return false;
        }

        var provider = await cache.GetOrAddAsync(
            $"Rights_{UserId}",
            async () =>
            {
                var providers = await providerRepository
                    .GetByFilter(p => p.UserId == UserId && p.Id == providerId);
                return providers?.Select(mapper.Map<ProviderDto>).FirstOrDefault();
            },
            TimeSpan.FromMinutes(5.0));

        // providerId == provider?.Id check is done in the filter,
        // so only need to check if the filter worked
        var result = provider is not null;

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access Provider ({ProviderId}) data",
                UserId,
                providerId);
        }

        return result;
    }

    private async Task<bool> ProviderAdminHasRights(string providerAdminId)
    {
        if (!IsDeputyOrProviderAdmin() || UserId != providerAdminId)
        {
            return false;
        }

        var providerAdmin = await cache.GetOrAddAsync(
            $"Rights_{UserId}",
            async () =>
            {
                var providerAdmins = await providerAdminRepository
                    .GetByFilter(p => p.UserId == UserId);
                return providerAdmins?.Select(mapper.Map<ProviderAdminProviderRelationDto>).FirstOrDefault();
            },
            TimeSpan.FromMinutes(5.0));

        // providerAdminId == providerAdmin?.UserId check is done before the filter,
        // so only need to check if the filter worked
        var result = providerAdmin is not null;

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access ProviderAdmin ({ProviderAdminId}) data",
                UserId,
                providerAdminId);
        }

        return result;
    }

    private async Task<bool> ProviderDeputyHasRights(Guid providerId)
    {
        if (!IsInSubRole(Subrole.ProviderDeputy))
        {
            return false;
        }

        var providerDeputy = await cache.GetOrAddAsync(
            $"Rights_{UserId}",
            async () =>
            {
                var providerAdmins = await providerAdminRepository
                    .GetByFilter(p => p.UserId == UserId);
                return providerAdmins?.Select(mapper.Map<ProviderAdminProviderRelationDto>).FirstOrDefault();
            },
            TimeSpan.FromMinutes(5.0));

        var result = providerDeputy?.ProviderId == providerId && providerDeputy.IsDeputy;

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access ProviderDeputy ({providerId}) data",
                UserId,
                providerId);
        }

        return result;
    }

    private async Task<bool> ProviderAdminHasWorkshopRights(Guid providerId, Guid workshopId)
    {
        if (!IsDeputyOrProviderAdmin())
        {
            return false;
        }

        var isUserRelatedAdmin = await cache.GetOrAddAsync(
            $"Rights_{UserId}_{providerId}_{workshopId}",
            async () =>
            {
                var providerAdmin = await providerAdminRepository.GetByIdAsync(UserId, providerId);

                if (providerAdmin is null)
                {
                    return false;
                }

                if (!providerAdmin.IsDeputy && workshopId != Guid.Empty)
                {
                    return providerAdmin.ManagedWorkshops.Any(w => w.Id == workshopId);
                }

                return true;
            },
            TimeSpan.FromMinutes(5.0));

        if (!isUserRelatedAdmin && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access Provider ({ProviderId}) and Workshop ({WorkshopId}) data",
                UserId,
                providerId,
                workshopId);
        }

        return isUserRelatedAdmin;
    }

    private bool IsInSubRole(Subrole subRole) =>
        user?.Identities
            .Any(identity =>
                identity.HasClaim(claim =>
                    string.Equals(
                        claim.Type,
                        IdentityResourceClaimsTypes.Subrole,
                        StringComparison.OrdinalIgnoreCase) &&
                    claim.Value.ToEnum(Subrole.None).Equals(subRole))) ?? false;
}