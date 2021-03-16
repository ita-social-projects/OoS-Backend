using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    /// Implements the interface with CRUD functionality for Workshop entity.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private readonly IEntityRepository<Workshop> repository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Workshop entity.</param>
        /// <param name="logger">Logger.</param>
        public WorkshopService(IEntityRepository<Workshop> repository, ILogger logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            logger.Information("Teacher creating was started.");

            var workshop = dto.ToDomain();

            var newWorkshop = await repository.Create(workshop).ConfigureAwait(false);

            return newWorkshop.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetAll()
        {
            logger.Information("Process of getting all Workshops started.");

            var workshops = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!workshops.Any()
                ? "Workshop table is empty."
                : "Successfully got all records from the Workshop table.");

            return workshops.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> GetById(long id)
        {
            logger.Information("Process of getting Teacher by id started.");

            var teacher = await repository.GetById(id).ConfigureAwait(false);

            if (teacher == null)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "The id cannot be greater than number of table entities.");
            }

            logger.Information($"Successfully got a Teacher with id = {id}.");

            return teacher.ToModel();
        }

        public async Task<IEnumerable<WorkshopDTO>> GetWorkshopsByOrganization(long id)
        {
            var workshops = await repository.GetByFilter(x => x.Organization.Id == id).ConfigureAwait(false);

            return workshops.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Update(WorkshopDTO dto)
        {
            logger.Information("Workshop updating was launched.");

            try
            {
                var workshop = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Workshop successfully updated.");

                return workshop.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Workshop in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Workshop deleting was launched.");

            var entity = new Workshop() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Workshop successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Teacher in the Db with such an id.");
                throw;
            }
        }
    }
}