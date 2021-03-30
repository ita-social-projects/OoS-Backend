using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for ParentController.
    /// </summary>
    public class ParentService : IParentService
    {
        private readonly IEntityRepository<Parent> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ParentService(IEntityRepository<Parent> entityRepository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = entityRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> Create(ParentDTO dto)
        {
            logger.Information("Parent creating was started");

            var parent = dto.ToDomain();

            var newParent = await repository.Create(parent).ConfigureAwait(false);

            logger.Information("Parent created successfully.");

            return newParent.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Parent deleting was launched.");

            var entity = new Parent() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Parent succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed.There is no parent in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ParentDTO>> GetAll()
        {
            logger.Information("Process of getting fll Parents started.");

            var parents = await this.repository.GetAll().ConfigureAwait(false);

            logger.Information(!parents.Any()
                ? "Parent table is empty."
                : "Successfully got all records from the Parent table.");

            return parents.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> GetById(long id)
        {
            logger.Information("Process of getting Parent by id started.");

            var parent = await repository.GetById((int)id).ConfigureAwait(false);

            if (parent == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfuly got a parent with id = {id}.");

            return parent.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> Update(ParentDTO dto)
        {
            logger.Information("Parent updating was launched.");

            try
            {
                var parent = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Parent succesfully updated.");

                return parent.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no parent in the Db with such an id.");
                throw;
            }
        }
    }
}
