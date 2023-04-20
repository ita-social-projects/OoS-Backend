using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public class PermissionsForRoleService : IPermissionsForRoleService
{
    private readonly IEntityRepository<long, PermissionsForRole> repository;
    private readonly ILogger<PermissionsForRoleService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionsForRoleService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public PermissionsForRoleService(
        IEntityRepository<long, PermissionsForRole> repository,
        ILogger<PermissionsForRoleService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PermissionsForRoleDTO>> GetAll()
    {
        logger.LogInformation("Getting all Permissions for Roles started.");

        var permissionsForRoles = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!permissionsForRoles.Any()
            ? "PermissionsForRole table is empty."
            : $"All {permissionsForRoles.Count()} records were successfully received from the PermissionsForRole table");

        return permissionsForRoles.Select(permissionsForRole => mapper.Map<PermissionsForRoleDTO>(permissionsForRole)).ToList();
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
        return mapper.Map<PermissionsForRoleDTO>(permissionsForRole);
    }

    /// <inheritdoc/>
    public async Task<PermissionsForRoleDTO> Create(PermissionsForRoleDTO dto)
    {
        logger.LogInformation($"Permissions for Role entity creating was started.");
        if (repository.GetByFilterNoTracking(p => p.RoleName == dto.RoleName).Any())
        {
            throw new ArgumentException("Permissions for this role exist in DB, you can't create one more", dto.RoleName);
        }

        var permissionsForRole = mapper.Map<PermissionsForRole>(dto);
        var newPermissionsForRole = await repository.Create(permissionsForRole).ConfigureAwait(false);
        logger.LogInformation($"Permissions for role with name {newPermissionsForRole.RoleName} created successfully.");

        return mapper.Map<PermissionsForRoleDTO>(newPermissionsForRole);
    }

    /// <inheritdoc/>
    public async Task<PermissionsForRoleDTO> Update(PermissionsForRoleDTO dto)
    {
        logger.LogInformation($"Updating Permissions For Role = {dto?.RoleName} started.");

        try
        {
            var permissionsForRole = await repository.Update(mapper.Map<PermissionsForRole>(dto)).ConfigureAwait(false);

            logger.LogInformation($"Permissions for Role with name = {permissionsForRole?.RoleName} updated succesfully.");

            return mapper.Map<PermissionsForRoleDTO>(permissionsForRole);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. PermissionsForRole with name = {dto?.RoleName} doesn't exist in the system.");
            throw;
        }
    }

}