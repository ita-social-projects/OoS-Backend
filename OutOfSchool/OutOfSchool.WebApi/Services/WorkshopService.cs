using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
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
        private readonly IEntityRepository<Rating> ratingRepository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly int roundDigits = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Workshop entity.</param>
        /// <param name="ratingRepository">Rating repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public WorkshopService(
            IEntityRepository<Workshop> repository, 
            IEntityRepository<Rating> ratingRepository,
            ILogger logger, 
            IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.ratingRepository = ratingRepository;
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

            var workshopsDTO = workshops.Select(x => x.ToModel()).ToList();

            foreach (var workshop in workshopsDTO)
            {
                var workshopRatings = await ratingRepository
                    .GetByFilter(rating => rating.EntityId == workshop.Id && rating.Type == RatingType.Workshop)
                    .ConfigureAwait(false);

                var ratingsSum = (float)workshopRatings.Sum(rating => rating.Rate);
                workshop.Rating = (float)Math.Round(ratingsSum / workshopRatings.Count(), roundDigits);
            }

            return workshopsDTO;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> GetById(long id)
        {
            logger.Information("Process of getting Teacher by id started.");

            var workshop = await repository.GetById(id).ConfigureAwait(false);

            if (workshop == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Teacher with id = {id}.");

            var workshopDTO = workshop.ToModel();

            var workshopRatings = await ratingRepository
                    .GetByFilter(rating => rating.EntityId == workshop.Id && rating.Type == RatingType.Workshop)
                    .ConfigureAwait(false);

            var ratingsSum = (float)workshopRatings.Sum(rating => rating.Rate);
            workshopDTO.Rating = (float)Math.Round(ratingsSum / workshopRatings.Count(), roundDigits);

            return workshopDTO;
        }

        public async Task<IEnumerable<WorkshopDTO>> GetWorkshopsByOrganization(long id)
        {
            var workshops = await repository.GetByFilter(x => x.Provider.Id == id).ConfigureAwait(false);

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