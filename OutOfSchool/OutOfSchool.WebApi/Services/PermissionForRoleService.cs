using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public class PermissionForRoleService : IPermissionsForRoleService
    {
        private readonly IEntityRepository<PermissionsForRole> repository;
        private readonly ILogger<PermissionForRoleService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionForRoleService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public PermissionForRoleService(IEntityRepository<PermissionsForRole> repository, ILogger<PermissionForRoleService> logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PermissionsForRoleDTO>> GetAll()
        {
            logger.LogInformation("Getting all Permissions for Roles started.");

            var permissionsForRoles = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!permissionsForRoles.Any()
                ? "PermissionsForRole table is empty."
                : $"All {permissionsForRoles.Count()} records were successfully received from the PermissionsForRole table");

            return permissionsForRoles.Select(permissionsForRoles => permissionsForRoles.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<PermissionsForRoleDTO> GetById(long id)
        {
            logger.LogInformation($"Getting Permissions for role by Id started. Looking Id = {id}.");

            var permissionsForRole = await repository.GetById(id).ConfigureAwait(false);

            if (permissionsForRole == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a permissionsForRole with name  {permissionsForRole.RoleName}.");

            return permissionsForRole.ToModel();
        }

        /// <inheritdoc/>
        public async Task<PermissionsForRoleDTO> Create(PermissionsForRoleDTO dto)
        {
            logger.LogInformation($"Permissions for Role - {dto?.RoleName} creating was started.");

            var permissionsForRole = dto.ToDomain();

            var newPermissionsForRole = await repository.Create(permissionsForRole).ConfigureAwait(false);

            logger.LogInformation($"Permissions for role with name {newPermissionsForRole.RoleName} created successfully.");

            return newPermissionsForRole.ToModel();
        }

        /// <inheritdoc/>
        public async Task<PermissionsForRoleDTO> Update(PermissionsForRoleDTO dto)
        {
            logger.LogInformation($"Updating Permissions For Role = {dto.RoleName} started.");

            try
            {
                var permissionsForRole = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.LogInformation($"Permissions for Role with name = {permissionsForRole?.RoleName} updated succesfully.");

                return permissionsForRole.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. PermissionsForRole with name = {dto.RoleName} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.LogInformation($"Deleting PermissionsForRole with Id = {id} started.");

            var permissionsForRole = await repository.GetById(id).ConfigureAwait(false);

            if (permissionsForRole == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer[$"PermissionsForRole with Id = {id} doesn't exist in the system"]);
            }

            await repository.Delete(permissionsForRole).ConfigureAwait(false);

            logger.LogInformation($"PermissionsForRole with Id = {id} with name {permissionsForRole.RoleName} succesfully deleted.");
        }
    }
}
