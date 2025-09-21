namespace OutOfSchool.AuthCommon.Services;
public class SystemUserInitializer : ISystemUserInitializer
{
    private readonly UserManager<User> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly ILogger<SystemUserInitializer> logger;

    public SystemUserInitializer(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger<SystemUserInitializer> logger)
    {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EnsureExistsAsync(CancellationToken cancellationToken = default)
    {
        var systemUserId = Constants.SystemUserConstants.SystemUserId;
        var systemRoleName = Constants.SystemUserConstants.SystemUserRole;
        var systemUserName = Constants.SystemUserConstants.SystemUserName;
        var systemEmail = Constants.SystemUserConstants.SystemUserEmail;

        if (!await roleManager.RoleExistsAsync(systemRoleName).ConfigureAwait(false))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(systemRoleName)).ConfigureAwait(false);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create system role '{RoleName}': {Errors}", systemRoleName, errors);
                throw new InvalidOperationException($"Failed to create system role '{systemRoleName}': {errors}");
            }
            logger.LogDebug("System role '{RoleName}' created successfully.", systemRoleName);
        }

        var existingUser = await userManager.FindByIdAsync(systemUserId).ConfigureAwait(false);

        if (existingUser is not null)
        {
            await EnsureInRoleAsync(existingUser, systemRoleName).ConfigureAwait(false);

            return;
        }

        var systemUser = new User
        {
            Id = systemUserId,
            UserName = systemUserName,
            FirstName = systemUserName,
            MiddleName = systemUserName,
            LastName = systemUserName,
            Email = systemEmail,
            EmailConfirmed = true,
            CreatingTime = DateTimeOffset.UtcNow,
            Role = systemRoleName,
            IsRegistered = false,
            IsBlocked = false
        };

        IdentityResult createResult;

        try
        {
            createResult = await userManager.CreateAsync(systemUser).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while creating system user '{UserName}'.", systemUserName);
            throw;
        }

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            logger.LogError("Failed to create system user '{UserName}': {Errors}", systemUserName, errors);
            throw new InvalidOperationException($"Failed to create system user '{systemUserName}': {errors}");
        }

        var createdUser = await userManager.FindByIdAsync(systemUserId).ConfigureAwait(false);
        if (createdUser == null)
        {
            logger.LogError("System user '{UserName}' was created but cannot be found.", systemUserName);
            throw new InvalidOperationException($"System user '{systemUserName}' was created but cannot be found.");
        }

        await EnsureInRoleAsync(createdUser, systemRoleName).ConfigureAwait(false);

        logger.LogInformation("System user '{UserName}' created and assigned to role '{RoleName}' successfully.", systemUserName, systemRoleName);
    }

    private async Task EnsureInRoleAsync(User user, string roleName)
    {
        if (!await userManager.IsInRoleAsync(user, roleName).ConfigureAwait(false))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, roleName).ConfigureAwait(false);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to assign user '{UserName}' to role '{RoleName}': {Errors}", user.UserName, roleName, errors);
                throw new InvalidOperationException($"Failed to assign user '{user.UserName}' to role '{roleName}': {errors}");
            }
            logger.LogDebug("User '{UserName}' assigned to role '{RoleName}' successfully.", user.UserName, roleName);
        }
    }
}
