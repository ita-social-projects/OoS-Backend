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
        var roleName = Constants.SystemUserConstants.SystemUserRole;
        var systemUserName = Constants.SystemUserConstants.SystemUserName;

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create system role '{RoleName}': {Errors}", roleName, errors);
                throw new InvalidOperationException($"Failed to create system role '{roleName}': {errors}");
            }
            logger.LogDebug("System role '{RoleName}' created successfully.", roleName);
        }

        var existingUser = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == systemUserId, cancellationToken).ConfigureAwait(false);

        if (existingUser is not null)
        {
            var user = await userManager.FindByIdAsync(systemUserId).ConfigureAwait(false);
            if (user == null)
            {
                logger.LogWarning("System user appeared in query but not found by FindById. Skipping.");
                return;
            }

            if (!await userManager.IsInRoleAsync(existingUser, roleName))
            {
                var addRole = await userManager.AddToRoleAsync(user, roleName).ConfigureAwait(false);
                if (!addRole.Succeeded)
                {
                    var errors = string.Join(", ", addRole.Errors.Select(e => e.Description));
                    logger.LogError("Failed to assign existing system user '{UserName}' to role '{RoleName}': {Errors}", systemUserName, roleName, errors);
                    throw new InvalidOperationException($"Failed to assign existing system user '{systemUserName}' to role '{roleName}': {errors}");
                }
                logger.LogDebug("Existing system user '{UserName}' assigned to role '{RoleName}' successfully.", systemUserName, roleName);
            }
            else
            {
                logger.LogDebug("System user '{UserName}' already exists and is in role '{RoleName}'. No action needed.", systemUserName, roleName);
            }

            return;
        }

        var systemUser = new User
        {
            Id = systemUserId,
            UserName = systemUserName,
            FirstName = systemUserName,
            MiddleName = systemUserName,
            LastName = systemUserName,
            Email = Constants.SystemUserConstants.SystemUserEmail,
            EmailConfirmed = true,
            CreatingTime = DateTimeOffset.UtcNow,
            Role = roleName,
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

        var addToRoleResult = await userManager.AddToRoleAsync(createdUser, roleName).ConfigureAwait(false);
        if (!addToRoleResult.Succeeded)
        {
            var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
            logger.LogError("Failed to assign system user '{UserName}' to role '{RoleName}': {Errors}", systemUserName, roleName, errors);
            throw new InvalidOperationException($"Failed to assign system user '{systemUserName}' to role '{roleName}': {errors}");
        }

        logger.LogInformation("System user '{UserName}' created and assigned to role '{RoleName}' successfully.", systemUserName, roleName);
    }
}
