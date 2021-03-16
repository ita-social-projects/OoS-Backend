using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Child entity.
    /// </summary>
    public class ChildService : IChildService
    {
        private readonly IEntityRepository<Child> repository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="repository">Repository for the Child entity.</param>
        /// <param name="logger">Logger.</param>
        public ChildService(IEntityRepository<Child> repository, ILogger logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Create(ChildDTO dto)
        {
            logger.Information("Child creating was started.");

            var child = dto.ToDomain();

            var newChild = await repository.Create(child).ConfigureAwait(false);

            return newChild.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAll()
        {
            logger.Information("Process of getting all Children started.");

            var children = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!children.Any()
                ? "Child table is empty."
                : "Successfully got all records from the Child table.");

            return children.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetById(long id)
        {
            logger.Information("Process of getting Child by id started.");

            var child = await repository.GetById(id).ConfigureAwait(false);

            if (child == null)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "The id cannot be greater than number of table entities.");
            }

            logger.Information($"Successfully got a Child with id = {id}.");

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Update(ChildDTO dto)
        {
            logger.Information("Child updating was launched.");

            try
            {
                var child = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Child successfully updated.'");

                return child.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Child in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Child deleting was launched.");

            var entity = new Child { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Child successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Child in the Db with such an id.");
                throw;
            }
        }
    }
}