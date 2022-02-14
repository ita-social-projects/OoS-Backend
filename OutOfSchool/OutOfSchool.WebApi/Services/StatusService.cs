using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for for InstitutionStatus entity.
    /// </summary>
    public class StatusService : IStatusService
    {

        private readonly IEntityRepository<InstitutionStatus> repository;
        private readonly ILogger<StatusService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public StatusService(IEntityRepository<InstitutionStatus> repository, ILogger<StatusService> logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }


        /// <inheritdoc/>
        public async Task<IEnumerable<InstitutionStatusDTO>> GetAll()
        {
            logger.LogInformation("Getting all Institution Statuses started.");

            var institutionStatuses = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!institutionStatuses.Any()
                ? "InstitutionStatus table is empty."
                : $"All {institutionStatuses.Count()} records were successfully received from the InstitutionStatus table");

            return institutionStatuses.Select(institutionStatus => institutionStatus.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<InstitutionStatusDTO> GetById(long id)
        {
            logger.LogInformation($"Getting InstitutionStatus by Id started. Looking Id = {id}.");

            var institutionStatus = await repository.GetById(id).ConfigureAwait(false);

            if (institutionStatus == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a institutionStatus with Id = {id}.");

            return institutionStatus.ToModel();
        }

        /// <inheritdoc/>
        public async Task<InstitutionStatusDTO> Create(InstitutionStatusDTO dto)
        {
            logger.LogInformation("InstitutionStatus creating was started.");

            var institutionStatus = dto.ToDomain();

            var newInstitutionStatus = await repository.Create(institutionStatus).ConfigureAwait(false);

            logger.LogInformation($"InstitutionStatus with Id = {newInstitutionStatus?.Id} created successfully.");

            return newInstitutionStatus.ToModel();
        }

        /// <inheritdoc/>
        public async Task<InstitutionStatusDTO> Update(InstitutionStatusDTO dto)
        {
            logger.LogInformation($"Updating InstitutionStatus with Id = {dto?.Id} started.");

            try
            {
                var institutionStatus = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.LogInformation($"InstitutionStatus with Id = {institutionStatus?.Id} updated succesfully.");

                return institutionStatus.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. InstitutionStatus with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.LogInformation($"Deleting InstitutionStatus with Id = {id} started.");

            var institutionStatus = await repository.GetById(id).ConfigureAwait(false);

            if (institutionStatus == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer[$"InstitutionStatus with Id = {id} doesn't exist in the system"]);
            }

            await repository.Delete(institutionStatus).ConfigureAwait(false);

            logger.LogInformation($"InstitutionStatus with Id = {id} succesfully deleted.");
        }
    }
}
