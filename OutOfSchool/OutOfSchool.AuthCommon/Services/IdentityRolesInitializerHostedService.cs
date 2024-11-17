using Microsoft.Extensions.Hosting;

namespace OutOfSchool.AuthCommon.Services;

/// <summary>
/// Hosted service that initializes default roles for correct work of authorization server.
/// </summary>
public class IdentityRolesInitializerHostedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<IdentityRolesInitializerHostedService> logger;

    public IdentityRolesInitializerHostedService(
        IServiceProvider serviceProvider,
        ILogger<IdentityRolesInitializerHostedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    /// <summary>
    /// Get default role names that should be initialized for correct work of authorization server.
    /// </summary>
    /// <returns>New array that contains default role names.</returns>
    public static string[] GetDefaultRoleNames() =>
    [
        "parent",
        "provider",
        "techadmin",
        "ministryadmin",
        "regionadmin",
        "areaadmin",
        "moderator",
        "employee"
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Adding default roles was cancelled");
            return;
        }

        using var scope = serviceProvider.CreateScope();

        var defaultRoleNames = GetDefaultRoleNames();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var existingRoleNames = await roleManager.Roles
            .Select(role => role.Name)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var newRoles = defaultRoleNames
            .Except(existingRoleNames)
            .Select(roleName => new IdentityRole { Name = roleName });

        var addedRolesCount = 0;

        logger.LogInformation("Starting adding default roles");

        foreach (var newRole in newRoles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Adding default roles was cancelled");
                break;
            }

            var roleName = newRole.Name;

            logger.LogInformation("Trying to add Role {RoleName}", roleName);

            var result = await roleManager.CreateAsync(newRole).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                LogRoleErrors(roleName, result);
                continue;
            }

            logger.LogInformation("Role {RoleName} was successfully created", roleName);

            addedRolesCount++;
        }

        if (addedRolesCount == 0)
        {
            logger.LogInformation("No roles were added");
        }
        else
        {
            logger.LogInformation("Successfully added {AddedRolesCount} roles", addedRolesCount);
        }

        logger.LogInformation("Finished adding default roles");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void LogRoleErrors(string? roleName, IdentityResult result)
    {
        logger.LogError("Failed to create Role {RoleName}", roleName);

        foreach (var error in result.Errors)
        {
            logger.LogError(
                "Error Code: {ErrorCode}, Description: {Description}",
                error.Code,
                error.Description);
        }
    }
}
