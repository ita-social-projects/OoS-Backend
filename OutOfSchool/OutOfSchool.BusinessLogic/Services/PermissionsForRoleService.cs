using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Initializes a new instance of the <see cref="PermissionsForRoleService"/> class.
/// </summary>
/// <param name="repository">Repository.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
public class PermissionsForRoleService(
    IEntityRepository<long, PermissionsForRole> repository,
    ILogger<PermissionsForRoleService> logger,
    IStringLocalizer<SharedResource> localizer
) : IPermissionsForRoleService
{

    /// <inheritdoc/>
    public async Task<IEnumerable<PermissionsForRoleDTO>> GetAll()
    {
        logger.LogInformation("Getting all Permissions for Roles started.");

        var permissionsForRoles = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!permissionsForRoles.Any()
            ? "PermissionsForRole table is empty."
            : $"All {permissionsForRoles.Count()} records were successfully received from the PermissionsForRole table");

        return permissionsForRoles.ToDto();
    }

    /// <inheritdoc/>
    public async Task<PermissionsForRoleDTO> GetByRole(string roleName)
    {
        logger.LogInformation($"Getting Permissions for role by roleName started. Looking role = {roleName}.");

        var permissionsForRole = (await repository.GetByFilter(p => p.RoleName == roleName).ConfigureAwait(false)).FirstOrDefault();
        if (permissionsForRole == null)
        {
            throw new ArgumentNullException(
                localizer[$"There are no packed permissions for role with such name - {roleName}"]);
        }

        logger.LogInformation($"Successfully got a permissionsForRole with name  {permissionsForRole.RoleName}.");
        return permissionsForRole.ToDto();
    }

    /// <inheritdoc/>
    public async Task<PermissionsForRoleDTO> Create(PermissionsForRoleDTO dto)
    {
        logger.LogInformation($"Permissions for Role entity creating was started.");
        if (repository.GetByFilterNoTracking(p => p.RoleName == dto.RoleName).Any())
        {
            throw new ArgumentException("Permissions for this role exist in DB, you can't create one more", dto.RoleName);
        }

        var permissionsForRole = dto.ToModel();
        var newPermissionsForRole = await repository.Create(permissionsForRole).ConfigureAwait(false);
        logger.LogInformation($"Permissions for role with name {newPermissionsForRole.RoleName} created successfully.");

        return newPermissionsForRole.ToDto();
    }

    /// <inheritdoc/>
    public async Task<PermissionsForRoleDTO> Update(PermissionsForRoleDTO dto)
    {
        logger.LogInformation($"Updating Permissions For Role = {dto?.RoleName} started.");

        try
        {
            var permissionsForRole = await repository.Update(dto.ToModel()).ConfigureAwait(false);

            logger.LogInformation($"Permissions for Role with name = {permissionsForRole?.RoleName} updated succesfully.");

            return permissionsForRole.ToDto();
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. PermissionsForRole with name = {dto?.RoleName} doesn't exist in the system.");
            throw;
        }
    }
}