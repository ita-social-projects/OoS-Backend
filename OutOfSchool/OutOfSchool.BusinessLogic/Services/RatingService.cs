using System.Linq.Expressions;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface for CRUD functionality for Rating entity.
/// </summary>
/// <param name="ratingRepository">Repository for Rating entity.</param>
/// <param name="workshopRepository">Repository for Workshop entity.</param>
/// <param name="parentRepository">Repository for Parent entity.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
/// <param name="operationWithObjectService">Service operation with rating.</param>
public class RatingService(
    IEntityRepositorySoftDeleted<long, Rating> ratingRepository,
    IWorkshopRepository workshopRepository,
    IParentRepository parentRepository,
    ILogger<RatingService> logger,
    IStringLocalizer<SharedResource> localizer,
    IOperationWithObjectService operationWithObjectService
) : IRatingService
{
    /// <inheritdoc/>
    public async Task<bool> IsReviewed(Guid parentId, Guid workshopId)
    {
        return await ratingRepository
                .Any(rating => rating.ParentId == parentId && rating.EntityId == workshopId)
                .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RatingDto>> GetAsync(OffsetFilter filter)
    {
        logger.LogInformation("Getting part of Ratings started.");

        var ratings = ratingRepository.Get(filter.From, filter.Size)
                                      .Include(r => r.Parent);

        logger.LogInformation(!ratings.Any()
            ? "Rating table is empty."
            : $"All {ratings.Count()} records were successfully received from the Rating table");

        return await AddParentInfoAsync(ratings.ToDto()).ConfigureAwait(false);
    }

    public async Task<IEnumerable<RatingDto>> GetAllAsync(Expression<Func<Rating, bool>> filter)
    {
        logger.LogInformation("Getting all ratings by filter started.");

        var ratings = await ratingRepository.GetByFilter(whereExpression: filter).ConfigureAwait(false);

        logger.LogInformation("Getting all ratings by filter finished.");

        return ratings.ToDto();
    }

    /// <inheritdoc/>
    public async Task<RatingDto> GetById(long id)
    {
        logger.LogInformation($"Getting Rating by Id started. Looking Id = {id}.");

        var rating = await ratingRepository.GetById(id).ConfigureAwait(false);

        if (rating == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Record with such Id = {id} don't exist in the system"]);
        }

        logger.LogInformation($"Successfully got a Rating with Id = {id}.");

        return rating.ToDto();
    }

    /// <inheritdoc/>
    public async Task<SearchResult<RatingDto>> GetAllByEntityId(Guid entityId, OffsetFilter filter)
    {
        logger.LogInformation($"Getting all Ratings with EntityId = {entityId} started.");

        filter ??= new OffsetFilter();
        var filterPredicate = PredicateBuilder.True<Rating>().And(r => r.EntityId == entityId);

        var totalAmount = await ratingRepository.Count(filterPredicate).ConfigureAwait(false);

        var ratings = await ratingRepository
            .Get(filter.From, filter.Size, whereExpression: filterPredicate)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(ratings.IsNullOrEmpty()
            ? "Rating table is empty."
            : $"All {ratings.Count} records with EntityId = {entityId} " +
              $"were successfully received from the Rating table");

        var ratingsDto = await AddParentInfoAsync(ratings.ToDto()).ConfigureAwait(false);

        var searchResult = new SearchResult<RatingDto>()
        {
            TotalAmount = totalAmount,
            Entities = ratingsDto.ToList(),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RatingDto>> GetAllWorshopsRatingByProvider(Guid id)
    {
        logger.LogInformation($"Getting all Worshops Ratings by ProviderId = {id} started.");

        var worshops = await workshopRepository.GetByFilter(x => x.Provider.Id == id).ConfigureAwait(false);

        var worshopsRating = new List<Rating>();

        foreach (var workshop in worshops)
        {
            worshopsRating.AddRange(await ratingRepository.GetByFilter(r => r.EntityId == workshop.Id).ConfigureAwait(false));
        }

        logger.LogInformation(!worshopsRating.Any()
            ? "Rating table is empty."
            : $"All {worshopsRating.Count} records with ProviderId = {id} " +
              $"were successfully received from the Rating table");

        return await AddParentInfoAsync(worshopsRating.ToDto()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RatingDto> GetParentRating(Guid parentId, Guid entityId)
    {
        logger.LogInformation($"Getting Rating for Parent started. Looking parentId = {parentId}, entityId = {entityId}.");

        var rating = (await ratingRepository
            .GetByFilter(r => r.ParentId == parentId
                              && r.EntityId == entityId)
            .ConfigureAwait(false)).FirstOrDefault();

        logger.LogInformation($"Successfully got a Rating for Parent with Id = {parentId}");

        return rating?.ToDto();
    }

    /// <inheritdoc/>
    public async Task<RatingDto> Create(RatingDto dto)
    {
        logger.LogInformation("Rating creating was started.");

        if (await CheckRatingCreation(dto).ConfigureAwait(false))
        {
            var rating = await ratingRepository.Create(dto.ToModel()).ConfigureAwait(false);

            logger.LogInformation($"Rating with Id = {rating?.Id} created successfully.");

            return rating.ToDto();
        }

        logger.LogInformation($"Rating already exists or entityId = {dto?.EntityId} isn't correct.");

        return null;
    }

    /// <inheritdoc/>
    public async Task<RatingDto> Update(RatingDto dto)
    {
        logger.LogInformation($"Updating Rating with Id = {dto?.Id} started.");

        if (await CheckRatingUpdate(dto).ConfigureAwait(false))
        {
            var rating = await ratingRepository.ReadAndUpdateWith(dto, RatingDtoExtensions.SetToModel).ConfigureAwait(false);

            logger.LogInformation($"Rating with Id = {rating?.Id} updated succesfully.");

            return rating.ToDto();
        }

        logger.LogInformation("Rating doesn't exist or couldn't change EntityId, Parent and Type.");

        return null;
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation("Deleting Rating with Id = {id} started.", id);

        var rating = await ratingRepository.GetById(id).ConfigureAwait(false);

        if (rating == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Rating with Id = {id} doesn't exist in the system"]);
        }

        await ratingRepository.RunInTransaction(async () => {
            await ratingRepository.Delete(rating).ConfigureAwait(false);

            var filter = new OperationWithObjectFilter() {
                OperationType = OperationWithObjectOperationType.RecalculateAverageRating,
                EntityId = rating.EntityId,
            };

            if (!await operationWithObjectService.Exists(filter)) 
            {
                await operationWithObjectService.Create(
                    OperationWithObjectOperationType.RecalculateAverageRating,
                    rating.EntityId,
                    OperationWithObjectEntityType.Workshop);
            }

            logger.LogInformation("Rating with Id = {id} succesfully deleted.", id);
        });
    }

    /// <summary>
    /// Checks that RatingDto is not null.
    /// </summary>
    /// <param name="dto">Rating Dto.</param>
    private void ValidateDto(RatingDto dto)
    {
        if (dto == null)
        {
            logger.LogInformation("Rating creating failed. Rating is null.");
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
            logger.LogInformation("Rating already exists");

            return false;
        }

        if (!await EntityExists(dto.EntityId).ConfigureAwait(false))
        {
            logger.LogInformation($"Record with entityId { dto.EntityId } don't exist in the system.");

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

        if (!await EntityExists(dto.EntityId).ConfigureAwait(false))
        {
            logger.LogInformation($"Record with entityId { dto.EntityId } don't exist in the system.");

            return false;
        }

        if (!RatingExistsWithId(dto))
        {
            logger.LogInformation($"Rating with Id = {dto.Id} doesn't exist.");

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
                              && r.ParentId == dto.ParentId)
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
                                        && r.Id == dto.Id);

        return result.Any();
    }

    /// <summary>
    /// Checks if Entity with such parameters already exists in the system.
    /// </summary>
    /// <param name="id">Entity Id.</param>
    /// <returns>True if Entity with such parameters already exists in the system and false otherwise.</returns>
    private async Task<bool> EntityExists(Guid id)
    {
        var workshop = workshopRepository.GetByFilterNoTracking(x => x.Id == id).FirstOrDefault();
        return workshop != null;
    }

    private async Task<IEnumerable<RatingDto>> AddParentInfoAsync(IEnumerable<RatingDto> ratingDtos)
    {
        var parentList = await parentRepository.GetByIdsAsync(ratingDtos.Select(rating => rating.ParentId).Distinct()).ConfigureAwait(false);
        var parents = parentList.ToDictionary(parent => parent.Id);

        return ratingDtos.Select(rating =>
        {
            rating.FirstName = parents[rating.ParentId].User.FirstName;
            rating.LastName = parents[rating.ParentId].User.LastName;
            return rating;
        });
    }
}