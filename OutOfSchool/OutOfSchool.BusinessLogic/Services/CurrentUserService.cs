#nullable enable

using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class CurrentUserService(
    ICurrentUser currentUser,
    IWorkshopRepository workshopRepository,
    IParentRepository parentRepository,
    IEntityRepositorySoftDeleted<Guid, Child> childRepository,
    ILogger<CurrentUserService> logger,
    ICacheService cache,
    IOptions<AppDefaultsConfig> options,
    ISensitiveEntityRepositorySoftDeleted<Moderator> moderatorRepository,
    ISensitiveEntityRepositorySoftDeleted<TechAdmin> techAdminRepository
) : ICurrentUserService
{
    private readonly AppDefaultsConfig options = options.Value;

    public Guid ProviderId
    {
        get
        {
            if (!IsInRole(Role.Provider) && !IsInRole(Role.Employee))
            {
                return Guid.Empty;
            }

            var id = HasClaim(Constants.ClaimTypes.ProviderId)
                ? GetClaimValue(Constants.ClaimTypes.ProviderId)
                : Guid.Empty.ToString();
            return Guid.Parse(id);
        }
    }

    public string UserId => currentUser.UserId;

    public string UserRole => currentUser.UserRole;

    public bool IsInRole(string role) => currentUser.IsInRole(role);

    public bool IsAuthenticated => currentUser.IsAuthenticated;

    public bool HasClaim(string type, Func<string, bool>? valueComparer = null) 
        => currentUser.HasClaim(type, valueComparer);
    
    public string? GetClaimValue(string claimType) => currentUser.GetClaimValue(claimType);

    public bool IsInRole(Role role) => role switch
    {
        Role.Provider => IsInRole("provider"),
        Role.Parent => IsInRole("parent"),
        Role.TechAdmin => IsInRole("techadmin"),
        Role.MinistryAdmin => IsInRole("ministryadmin"),
        Role.RegionAdmin => IsInRole("regionadmin"),
        Role.AreaAdmin => IsInRole("areaadmin"),
        Role.Moderator => IsInRole("moderator"),
        Role.Employee => IsInRole("employee"),
        _ => throw new NotImplementedException("Role not handled"),
    };

    public bool IsAdmin() => IsInRole(Role.TechAdmin) || IsInRole(Role.MinistryAdmin) || IsInRole(Role.RegionAdmin) ||
                             IsInRole(Role.AreaAdmin);

    public bool IsTechAdmin() => IsInRole(Role.TechAdmin);

    public bool IsMinistryAdmin() => IsInRole(Role.MinistryAdmin);

    public bool IsRegionAdmin() => IsInRole(Role.RegionAdmin);

    public bool IsAreaAdmin() => IsInRole(Role.AreaAdmin);

    public bool IsModerator() => IsInRole(Role.Moderator);

    public async Task UserHasRights(params IUserRights[] userTypes)
    {
        var userHasRights = false;
        if (IsAuthenticated)
        {
            var parent = userTypes.OfType<ParentRights>().FirstOrDefault();
            var provider = userTypes.OfType<ProviderRights>().FirstOrDefault();
            var employee = userTypes.OfType<EmployeeRights>().FirstOrDefault();
            var employeeWorkshop = userTypes.OfType<EmployeeWorkshopRights>().FirstOrDefault();
            var deputyDirector = userTypes.OfType<DeputyDirectorRights>().FirstOrDefault();
            var moderator = userTypes.OfType<ModeratorRights>().FirstOrDefault();
            var techAdmin = userTypes.OfType<TechAdminRights>().FirstOrDefault();

            var result = await Task.WhenAll(
                new List<Task<bool>>
                    {
                        UserHasRights(parent),
                        UserHasRights(provider),
                        UserHasRights(employee),
                        UserHasRights(employeeWorkshop),
                        UserHasRights(deputyDirector),
                        UserHasRights(moderator),
                        UserHasRights(techAdmin),
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
            EmployeeRights employee => EmployeeHasRights(employee.providerId),
            DeputyDirectorRights deputy => DeputyDirectorHasRights(deputy.providerId),
            ProviderRights provider => ProviderHasRights(provider.providerId),
            EmployeeWorkshopRights employeeWorkshop => EmployeeWorkshopRights(employeeWorkshop.workshopId),
            ModeratorRights => ModeratorHasRights(),
            TechAdminRights => TechAdminHasRights(),
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
                return parents?.FirstOrDefault()?.ToDto();
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
                    return children?.FirstOrDefault()?.ToDto();
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

    // Keep as task for logic consistency
    private Task<bool> ProviderHasRights(Guid providerId)
    {
        if (!IsInRole(Role.Provider))
        {
            return Task.FromResult(false);
        }

        var result = HasClaim(Constants.ClaimTypes.ProviderId) && GetClaimValue(Constants.ClaimTypes.ProviderId) == providerId.ToString();

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access Provider ({ProviderId}) data",
                UserId,
                providerId);
        }

        return Task.FromResult(result);
    }

    private Task<bool> EmployeeHasRights(Guid providerId)
    {
        if (!IsInRole(Role.Employee))
        {
            return Task.FromResult(false);
        }

        var result = HasClaim(Constants.ClaimTypes.ProviderId) && GetClaimValue(Constants.ClaimTypes.ProviderId) == providerId.ToString();

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access Provider ({ProviderId}) data as employee",
                UserId,
                providerId);
        }

        return Task.FromResult(result);
    }

    private Task<bool> DeputyDirectorHasRights(Guid providerId)
    {
        if (!IsInRole(Role.Employee))
        {
            return Task.FromResult(false);
        }

        var isProviderEmployee = HasClaim(Constants.ClaimTypes.ProviderId) && GetClaimValue(Constants.ClaimTypes.ProviderId) == providerId.ToString();
        var isDeputyDirector = HasClaim(Constants.ClaimTypes.IsDeputy) && bool.Parse(GetClaimValue(Constants.ClaimTypes.IsDeputy));
        var result = isProviderEmployee && isDeputyDirector;

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access Provider ({ProviderId}) data as deputy director",
                UserId,
                providerId);
        }

        return Task.FromResult(result);
    }
    
    private async Task<bool> EmployeeWorkshopRights(Guid workshopId)
    {
        if (!IsInRole(Role.Employee) && !IsInRole(Role.Provider))
        {
            return false;
        }
        
        var providerId = GetClaimValue(Constants.ClaimTypes.ProviderId);

        if (providerId.IsNullOrEmpty())
        {
            return false;
        }

        var isUserRelatedEmployee = await cache.GetOrAddAsync(
            $"Rights_{UserId}_{providerId}_{workshopId}",
            async () =>
            {
                var workshopProviderId = await workshopRepository
                    .GetByFilterNoTracking(w => w.Id == workshopId)
                    .Select(w => w.ProviderId)
                    .SingleOrDefaultAsync()
                    .ConfigureAwait(false);

                return workshopProviderId != Guid.Empty && workshopProviderId.ToString() == providerId;
            },
            TimeSpan.FromMinutes(5.0));

        if (!isUserRelatedEmployee && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access Provider ({ProviderId}) and Workshop ({WorkshopId}) data",
                UserId,
                providerId,
                workshopId);
        }

        return isUserRelatedEmployee;
    }

    private async Task<bool> ModeratorHasRights()
    {
        if (!IsModerator())
        {
            return false;
        }

        var individualIdString = GetClaimValue(Constants.ClaimTypes.IndividualId);
        if (!Guid.TryParse(individualIdString, out var individualId))
        {
            if (options.AccessLogEnabled)
            {
                logger.LogWarning(
                    "Unauthorized access: User ({UserId}) has invalid individual ID claim)", UserId);
            }
            return false;
        }

        var result = await moderatorRepository.Any(m => m.Id == individualId);

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access data as Moderator)", UserId);
        }

        return result;
    }

    private async Task<bool> TechAdminHasRights()
    {
        if (!IsTechAdmin())
        {
            return false;
        }

        var individualIdString = GetClaimValue(Constants.ClaimTypes.IndividualId);
        if (!Guid.TryParse(individualIdString, out var individualId))
        {
            if (options.AccessLogEnabled)
            {
                logger.LogWarning(
                    "Unauthorized access: User ({UserId}) has invalid individual ID claim)", UserId);
            }
            return false;
        }

        var result = await techAdminRepository.Any(ta => ta.Id == individualId);

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning("Unauthorized access: User ({UserId}) tried to access data as TechAdmin)", UserId);
        }

        return result;
    }
}