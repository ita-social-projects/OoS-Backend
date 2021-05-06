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
    /// Implements the interface for CRUD functionality for Rating entity.
    /// </summary>
    public class RatingService : IRatingService
    {
        private readonly IEntityRepository<Rating> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Rating entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public RatingService(
            IEntityRepository<Rating> repository,
            ILogger logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RatingDTO>> GetAll()
        {
            logger.Information("Process of getting all Rating started.");

            var ratings = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!ratings.Any()
                ? "Rating table is empty."
                : "Successfully got all records from the Rating table.");

            return ratings.Select(r => r.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<RatingDTO> GetById(long id)
        {
            logger.Information("Process of getting Rating by id started.");

            var rating = await repository.GetById(id).ConfigureAwait(false);

            if (rating == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Rating with id = {id}.");

            return rating.ToModel();
        }

        /// <inheritdoc/>
        public async Task<RatingDTO> Create(RatingDTO dto)
        {
            logger.Information("Rating creating was started.");

            Check(dto);

            var rating = dto.ToDomain();

            if (!RatingExist(rating).Result)
            {
                var newRating = await repository.Create(rating).ConfigureAwait(false);

                logger.Information("Rating created successfully.");

                return newRating.ToModel();
            }
            else
            {
                logger.Information("Rating already exist.");

                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<RatingDTO> Update(RatingDTO dto)
        {
            logger.Information("Rating updating was launched.");
            Check(dto);

            var rating = dto.ToDomain();
            var ratingResult = RatingExistResult(rating).Result.First();

            try
            {
                if (RatingExist(rating).Result
                    && rating.EntityId == ratingResult.EntityId
                    && rating.Parent == ratingResult.Parent
                    && rating.Type == ratingResult.Type)
                {
                    var updatedRating = await repository.Update(rating).ConfigureAwait(false);

                    logger.Information("Rate successfully updated.");

                    return updatedRating.ToModel();
                }
                else
                {
                    logger.Information("Rating not exist or can't change EntityId, Parent and Type.");

                    return null;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Rating in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public Task Delete(long id)
        {
            throw new NotImplementedException();
        }

        private void Check(RatingDTO dto)
        {
            if (dto == null)
            {
                logger.Information("Rating creating failed. Rating was null.");
                throw new ArgumentNullException(nameof(dto), localizer["Rating was null."]);
            }
        }

        private async Task<bool> RatingExist(Rating rating)
        {
            var result = await repository
                .GetByFilter(r => r.EntityId == rating.EntityId
                               && r.Parent == rating.Parent
                               && r.Type == rating.Type)
                .ConfigureAwait(false);

            return result.Any();
        }

        private async Task<IEnumerable<Rating>> RatingExistResult(Rating rating)
        {
            return await repository
                .GetByFilter(r => r.EntityId == rating.EntityId
                               && r.Parent == rating.Parent
                               && r.Type == rating.Type)
                .ConfigureAwait(false);
        }
    }
}
