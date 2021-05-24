using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    /// Implements the interface for CRUD functionality for Rating entity.
    /// </summary>
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository ratingRepository;
        private readonly IEntityRepository<Workshop> workshopRepository;
        private readonly IProviderRepository providerRepository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly int roundToDigits = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingService"/> class.
        /// </summary>
        /// <param name="ratingRepository">Repository for Rating entity.</param>
        /// <param name="workshopRepository">Repository for Workshop entity.</param>
        /// <param name="providerRepository">Repository for Provider entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public RatingService(
            IRatingRepository ratingRepository,
            IEntityRepository<Workshop> workshopRepository,
            IProviderRepository providerRepository,
            ILogger logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.ratingRepository = ratingRepository;
            this.workshopRepository = workshopRepository;
            this.providerRepository = providerRepository;
            this.logger = logger;
            this.localizer = localizer;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RatingDto>> GetAll()
        {
            logger.Information("Getting all Ratings started.");

            var ratings = await ratingRepository.GetAll().ConfigureAwait(false);

            logger.Information(!ratings.Any()
                ? "Rating table is empty."
                : $"All {ratings.Count()} records were successfully received from the Rating table");

            return ratings.Select(r => r.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<RatingDto> GetById(long id)
        {
            logger.Information($"Getting Rating by Id started. Looking Id = {id}.");

            var rating = await ratingRepository.GetById(id).ConfigureAwait(false);

            if (rating == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer[$"Record with such Id = {id} don't exist in the system"]);
            }

            logger.Information($"Successfully got a Rating with Id = {id}.");

            return rating.ToModel();
        }

        /// <inheritdoc/>
        public async Task<RatingDto> GetParentRating(long parentId, long entityId, RatingType type)
        {
            logger.Information($"Getting Rating for Parent started. Looking parentId = {parentId}, entityId = {entityId} and type = {type}.");

            var rating = await ratingRepository
                .GetByFilter(r => r.ParentId == parentId
                               && r.EntityId == entityId
                               && r.Type == type)
                .ConfigureAwait(false);

            logger.Information($"Successfully got a Rating for Parent with Id = {parentId}");

            return rating.FirstOrDefault().ToModel();
        }

        /// <inheritdoc/>
        public float GetAverageRating(long entityId, RatingType type)
        {
            return (float)Math.Round(ratingRepository.GetAverageRating(entityId, type), roundToDigits);
        }

        /// <inheritdoc/>
        public Dictionary<long, float> GetAverageRatingForRange(IEnumerable<long> entities, RatingType type)
        {
            var entitiesRating = ratingRepository.GetAverageRatingForEntities(entities, type);

            var formattedEntities = new Dictionary<long, float>(entitiesRating.Count);

            foreach (var entity in entitiesRating)
            {
                formattedEntities.Add(entity.Key, (float)Math.Round(entity.Value, roundToDigits));
            }

            return formattedEntities;
        }

        /// <inheritdoc/>
        public async Task<RatingDto> Create(RatingDto dto)
        {
            logger.Information("Rating creating was started.");

            if (await CheckRatingCreation(dto).ConfigureAwait(false))
            {
                var rating = await ratingRepository.Create(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Rating with Id = {rating?.Id} created successfully.");

                return rating.ToModel();
            }
            else
            {
                logger.Information($"Rating already exists or entityId = {dto?.EntityId} isn't correct.");

                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<RatingDto> Update(RatingDto dto)
        {
            logger.Information($"Updating Rating with Id = {dto?.Id} started.");

            if (await CheckRatingUpdate(dto).ConfigureAwait(false))
            {
                var rating = await ratingRepository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Rating with Id = {rating?.Id} updated succesfully.");

                return rating.ToModel();
            }
            else
            {
                logger.Information("Rating doesn't exist or couldn't change EntityId, Parent and Type.");

                return null;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Rating with Id = {id} started.");

            var rating = await ratingRepository.GetById(id).ConfigureAwait(false);

            if (rating == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer[$"Rating with Id = {id} doesn't exist in the system"]);
            }

            await ratingRepository.Delete(rating).ConfigureAwait(false);

            logger.Information($"Rating with Id = {id} succesfully deleted.");
        }

        /// <summary>
        /// Checks that RatingDto is not null.
        /// </summary>
        /// <param name="dto">Rating Dto.</param>
        private void ValidateDto(RatingDto dto)
        {
            if (dto == null)
            {
                logger.Information("Rating creating failed. Rating is null.");
                throw new ArgumentNullException(nameof(dto), localizer["Rating is null."]);
            }
        }

        /// <summary>
        /// Checks if Rating with such parameters could be created.
        /// </summary>
        /// <param name="dto">Rating Dto.</param>
        /// <returns>True if Rating with such parameters could be added to the system and false otherwise.</returns>
        private async Task<bool> CheckRatingCreation(RatingDto dto)
        {
            ValidateDto(dto);

            if (await RatingExists(dto).ConfigureAwait(false))
            {
                logger.Information("Rating already exists");

                return false;
            }

            if (!await EntityExists(dto.EntityId, dto.Type).ConfigureAwait(false))
            {
                logger.Information($"Record with entityId { dto.EntityId } " +
                    $"and Type { dto.Type } don't exist in the system.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if Rating with such parameters could be updated.
        /// </summary>
        /// <param name="dto">Rating Dto.</param>
        /// <returns>True if Rating with such parameters could be updated and false otherwise.</returns>
        private async Task<bool> CheckRatingUpdate(RatingDto dto)
        {
            ValidateDto(dto);

            if (!await EntityExists(dto.EntityId, dto.Type).ConfigureAwait(false))
            {
                logger.Information($"Record with entityId { dto.EntityId } " +
                    $"and Type { dto.Type } don't exist in the system.");

                return false;
            }

            if (!RatingExistsWithId(dto))
            {
                logger.Information($"Rating with Id = {dto.Id} doesn't exist.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if Rating with such parameters already exists in the system.
        /// </summary>
        /// <param name="dto">Rating Dto.</param>
        /// <returns>True if Rating with such parameters already exists in the system and false otherwise.</returns>
        private async Task<bool> RatingExists(RatingDto dto)
        {
            var rating = await ratingRepository
                .GetByFilter(r => r.EntityId == dto.EntityId
                               && r.ParentId == dto.ParentId
                               && r.Type == dto.Type)
                .ConfigureAwait(false);

            return rating.Any();
        }

        /// <summary>
        /// Checks if Rating with such parameters already exists in the system.
        /// </summary>
        /// <param name="dto">Rating Dto.</param>
        /// <returns>True if Rating with such parameters already exists in the system and false otherwise.</returns>
        private bool RatingExistsWithId(RatingDto dto)
        {
            var result = ratingRepository
                    .GetByFilterNoTracking(r => r.EntityId == dto.EntityId
                                             && r.ParentId == dto.ParentId
                                             && r.Type == dto.Type
                                             && r.Id == dto.Id);

            return result.Any();
        }

        /// <summary>
        /// Checks if Entity with such parameters already exists in the system.
        /// </summary>
        /// <param name="id">Entity Id.</param>
        /// <param name="type">Entity type.</param>
        /// <returns>True if Entity with such parameters already exists in the system and false otherwise.</returns>
        private async Task<bool> EntityExists(long id, RatingType type)
        {
            switch (type)
            {
                case RatingType.Provider:
                    Provider provider = await providerRepository.GetById(id).ConfigureAwait(false);
                    return provider != null;

                case RatingType.Workshop:
                    Workshop workshop = await workshopRepository.GetById(id).ConfigureAwait(false);
                    return workshop != null;

                default:
                    return false;
            }
        }
    }
}
