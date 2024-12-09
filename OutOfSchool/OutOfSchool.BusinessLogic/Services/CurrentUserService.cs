#nullable enable

using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ICurrentUser currentUser;
    private readonly IParentRepository parentRepository;
    private readonly IEntityRepositorySoftDeleted<Guid, Child> childRepository;
    private readonly IProviderRepository providerRepository;
    private readonly IEmployeeRepository employeeRepository;
    private readonly ILogger<CurrentUserService> logger;
    private readonly ICacheService cache;
    private readonly AppDefaultsConfig options;
    private readonly IMapper mapper;

    public CurrentUserService(
        ICurrentUser currentUser,
        IProviderRepository providerRepository,
        IEmployeeRepository employeeRepository,
        IParentRepository parentRepository,
        IEntityRepositorySoftDeleted<Guid, Child> childRepository,
        ILogger<CurrentUserService> logger,
        ICacheService cache,
        IOptions<AppDefaultsConfig> options,
        IMapper mapper)
    {
        this.currentUser = currentUser;
        this.providerRepository = providerRepository;
        this.employeeRepository = employeeRepository;
        this.parentRepository = parentRepository;
        this.childRepository = childRepository;
        this.logger = logger;
        this.cache = cache;
        this.options = options.Value;
        this.mapper = mapper;
    }

    public string UserId => currentUser.UserId;

    public string UserRole => currentUser.UserRole;

    public bool IsInRole(string role) => currentUser.IsInRole(role);

    public bool IsAuthenticated => currentUser.IsAuthenticated;

    public bool HasClaim(string type, Func<string, bool>? valueComparer = null) =>
        currentUser.HasClaim(type, valueComparer);

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
    
    public bool IsEmployee() => IsInRole(Role.Employee);

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
            var providerAdmin = userTypes.OfType<EmployeeRights>().FirstOrDefault();
            var providerAdminWorkshop = userTypes.OfType<EmployeeWorkshopRights>().FirstOrDefault();

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
            ParentRights parent => ParentHasRights(parent.parentId, parent.childId),
            EmployeeRights providerAdmin => ProviderAdminHasRights(providerAdmin.employeeId),
            EmployeeWorkshopRights providerAdminWorkshop => this.EmployeeHasWorkshopRights(
                providerAdminWorkshop.providerId, providerAdminWorkshop.workshopId),
            ProviderRights provider => ProviderHasRights(provider.providerId),
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
        if (!IsInRole(Role.Provider) || this.IsEmployee())
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
        if (!this.IsEmployee() || UserId != providerAdminId)
        {
            return false;
        }

        var providerAdmin = await cache.GetOrAddAsync(
            $"Rights_{UserId}",
            async () =>
            {
                var providerAdmins = await employeeRepository
                    .GetByFilter(p => p.UserId == UserId);
                return providerAdmins?.Select(mapper.Map<EmployeeProviderRelationDto>).FirstOrDefault();
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

    private async Task<bool> EmployeeHasRights(Guid providerId)
    {
        if (!IsInRole(Role.Employee))
        {
            return false;
        }

        var providerDeputy = await cache.GetOrAddAsync(
            $"Rights_{UserId}",
            async () =>
            {
                var employees = await employeeRepository
                    .GetByFilter(p => p.UserId == UserId);
                return employees?.Select(mapper.Map<EmployeeProviderRelationDto>).FirstOrDefault();
            },
            TimeSpan.FromMinutes(5.0));

        var result = providerDeputy?.ProviderId == providerId;

        if (!result && options.AccessLogEnabled)
        {
            logger.LogWarning(
                "Unauthorized access: User ({UserId}) tried to access Employee ({providerId}) data",
                UserId,
                providerId);
        }

        return result;
    }

    private async Task<bool> EmployeeHasWorkshopRights(Guid providerId, Guid workshopId)
    {
        if (!IsInRole(Role.Employee))
        {
            return false;
        }

        var isUserRelatedEmployee = await cache.GetOrAddAsync(
            $"Rights_{UserId}_{providerId}_{workshopId}",
            async () =>
            {
                var employee = await employeeRepository.GetByIdAsync(UserId, providerId);

                if (employee is null)
                {
                    return false;
                }

                if (workshopId != Guid.Empty)
                {
                    return employee.ManagedWorkshops.Any(w => w.Id == workshopId);
                }

                return true;
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
}