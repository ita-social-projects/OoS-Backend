namespace OutOfSchool.AuthCommon.Services;
public class SystemUserInitializer<TUser> : ISystemUserInitializer
    where TUser : User, new()
{
    private readonly UserManager<User> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly ILogger<SystemUserInitializer<User>> logger;

    public SystemUserInitializer(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger<SystemUserInitializer<User>> logger)
    {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        this.logger = logger;
    }

    public async Task EnsureExistsAsync(CancellationToken cancellationToken = default)
    {
        var systemUserId = Constants.SystemUserConstants.SystemUserId;
        var roleName = Constants.SystemUserConstants.SystemUserRole;
        var systemUserName = Constants.SystemUserConstants.SystemUserName;

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create system role '{RoleName}': {Errors}", roleName, errors);
                throw new InvalidOperationException($"Failed to create system role '{roleName}': {errors}");
            }
            logger.LogInformation("System role '{RoleName}' created successfully.", roleName);
        }

        var existingUser = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == systemUserId).ConfigureAwait(false);

        if (existingUser is not null)
        {
            var user = await userManager.FindByIdAsync(systemUserId).ConfigureAwait(false);
            if (user == null)
            {
                logger.LogWarning("System user appeared in query but not found by FindById. Skipping.");
                return;
            }
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
            createResult = await userManager.CreateAsync(systemUser);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Race detected creating system user — another instance likely created it first.");
            createResult = IdentityResult.Success;
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
