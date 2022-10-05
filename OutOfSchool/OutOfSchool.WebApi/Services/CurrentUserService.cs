#nullable enable

using System.Security.Claims;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? user;
    private readonly IParentService parentService;
    private readonly IProviderService providerService;
    private readonly IProviderAdminService providerAdminService;
    private readonly ILogger<CurrentUserService> logger;
    private readonly ICacheService cache;
    private readonly AppDefaultsConfig options;

    public CurrentUserService(
        ClaimsPrincipal? user,
        IProviderService providerService,
        IProviderAdminService providerAdminService,
        IParentService parentService,
        ILogger<CurrentUserService> logger,
        ICacheService cache,
        IOptions<AppDefaultsConfig> options)
    {
        this.user = user;
        this.providerService = providerService;
        this.providerAdminService = providerAdminService;
        this.parentService = parentService;
        this.logger = logger;
        this.cache = cache;
        this.options = options.Value;
    }

    public string UserId => user?.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? string.Empty;

    public bool IsInRole(Role role) => role switch
    {
        Role.Provider => user?.IsInRole("provider") ?? false,
        Role.Parent => user?.IsInRole("parent") ?? false,
        Role.TechAdmin => user?.IsInRole("techadmin") ?? false,
        Role.MinistryAdmin => user?.IsInRole("ministryadmin") ?? false,
        _ => throw new NotImplementedException("Role not handled")
    };

    public bool IsDeputyOrProviderAdmin() =>
        IsInRole(Role.Provider) &&
        (IsInSubRole(Subrole.ProviderDeputy) || IsInSubRole(Subrole.ProviderAdmin));

    public async Task UserHasRights(params IUserRights[] userTypes)
    {
        var userHasRights = false;
        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var parent = userTypes.OfType<ParentRights>().FirstOrDefault();
            var provider = userTypes.OfType<ProviderRights>().FirstOrDefault();
            var providerAdmin = userTypes.OfType<ProviderAdminRights>().FirstOrDefault();
            var providerAdminWorkshop = userTypes.OfType<ProviderAdminWorkshopRights>().FirstOrDefault();

            var result = await Task.WhenAll(
                new List<Task<bool>>
                    {
                        UserHasRights(parent),
                        UserHasRights(provider),
                        UserHasRights(providerAdmin),
                        UserHasRights(providerAdminWorkshop),
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
            ParentRights parent => ParentHasRights(parent.parentId),
            ProviderAdminRights providerAdmin => ProviderAdminHasRights(providerAdmin.providerAdminId),
            ProviderAdminWorkshopRights providerAdminWorkshop => ProviderAdminHasWorkshopRights(providerAdminWorkshop.providerId, providerAdminWorkshop.workshopId),
            ProviderRights provider => ProviderHasRights(provider.providerId),
            null => Task.FromResult(false),
            _ => throw new NotImplementedException("Unknown user rights type")
        };

    private async Task<bool> ParentHasRights(Guid parentId)
    {
        if (!IsInRole(Role.Parent))
        {
            return false;
        }

        var parent = await cache.GetOrAddAsync($"Rights_{UserId}", () => parentService.GetByUserId(UserId), TimeSpan.FromMinutes(5.0));

        var result = parent.Id == parentId;

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

        var provider = await cache.GetOrAddAsync($"Rights_{UserId}", () => providerService.GetByUserId(UserId), TimeSpan.FromMinutes(5.0));

        var result = providerId == (provider?.Id ?? Guid.Empty);

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
        if (!IsDeputyOrProviderAdmin())
        {
            return false;
        }

        var providerAdmin = await cache.GetOrAddAsync($"Rights_{UserId}", () => providerAdminService.GetById(UserId), TimeSpan.FromMinutes(5.0));

        var result = providerAdmin.UserId == providerAdminId;

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access ProviderAdmin ({ProviderAdminId}) data",
                UserId,
                providerAdminId);
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
            () => providerAdminService.CheckUserIsRelatedProviderAdmin(UserId, providerId, workshopId),
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