#nullable enable

using System.Security.Claims;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using ProviderType = OutOfSchool.Common.Models.ProviderType;

namespace OutOfSchool.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? user;
    private readonly IParentService parentService;
    private readonly IProviderService providerService;
    private readonly IProviderAdminService providerAdminService;

    public CurrentUserService(
        ClaimsPrincipal? user,
        IProviderService providerService,
        IProviderAdminService providerAdminService,
        IParentService parentService)
    {
        this.user = user;
        this.providerService = providerService;
        this.providerAdminService = providerAdminService;
        this.parentService = parentService;

        // TODO: add caching as this service is going to be called everytime use makes requests
        // TODO: so probably can cache extended user info for quick validations
    }

    public string UserId => user?.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? string.Empty;

    public bool IsInRole(Role role) => role switch
    {
        Role.Provider => user?.IsInRole("provider") ?? false,
        Role.Parent => user?.IsInRole("parent") ?? false,
        Role.TechAdmin => user?.IsInRole("techadmin") ?? false,
        Role.MinistryAdmin => user?.IsInRole("ministryadmin") ?? false,
        _ => false
    };

    // TODO: in code we check multiple permissions (for example parent and provider can edit application status)
    // TODO: either separate method calls with IsInRole check, or array like here?
    public async Task UserHasRights(params IUserType[] userTypes)
    {
        // TODO: double check role?
        var userHasRights = false;
        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var parent = userTypes.Select(t => t as ParentType).FirstOrDefault(p => p is not null);
            var provider = userTypes.Select(t => t as ProviderType).FirstOrDefault(p => p is not null);
            var providerAdmin = userTypes.Select(t => t as ProviderAdminType).FirstOrDefault(p => p is not null);

            var result = await Task.WhenAll(
                new List<Task<bool>>
                    {
                        UserHasRights(parent),
                        UserHasRights(provider),
                        UserHasRights(providerAdmin),
                    }
                    .Select(Execute));
            userHasRights = result.Aggregate(false, (acc, next) => acc || next);
        }

        if (!userHasRights)
        {
            // TODO: copied from existing. Need custom exception
            throw new ArgumentException("User has no rights to perform operation");
        }
    }

    private static async Task<bool> Execute(Task<bool> original)
    {
        try
        {
            return await original;
        }
        catch (Exception)
        {
            // TODO: Logging?
            return false;
        }
    }

    private Task<bool> UserHasRights<T>(T? userType)
        where T : IUserType
        => userType switch
        {
            ParentType parent => ParentHasRights(parent.parentId),
            ProviderType provider => ProviderHasRights(provider.providerAdminId),
            ProviderAdminType providerAdmin => ProviderAdminHasRights(
                providerAdmin.providerId,
                providerAdmin.workshopId),
            _ => Task.FromResult(false)
        };

    private async Task<bool> ParentHasRights(Guid parentId)
    {
        var parent = await parentService.GetByUserId(UserId).ConfigureAwait(false);

        return parent.Id == parentId;
    }

    // TODO: initial logic is probably messed up
    private async Task<bool> ProviderHasRights(string providerAdminId)
    {
        var providerAdmin = await providerAdminService.GetById(UserId).ConfigureAwait(false);

        return providerAdmin.UserId == providerAdminId;
    }

    // TODO: initial logic is probably messed up
    private async Task<bool> ProviderAdminHasRights(Guid providerId, Guid workshopId)
    {
        try
        {
            var provider = await providerService.GetByUserId(UserId).ConfigureAwait(false);
            return provider.Id == providerId;
        }
        catch (ArgumentException)
        {
            var isUserRelatedAdmin = await providerAdminService
                .CheckUserIsRelatedProviderAdmin(UserId, providerId, workshopId).ConfigureAwait(false);
            if (!isUserRelatedAdmin)
            {
                return false;
            }
        }

        // TODO: probably messed up logic, was just ctrl+c/ctrl+v
        return true;
    }
}