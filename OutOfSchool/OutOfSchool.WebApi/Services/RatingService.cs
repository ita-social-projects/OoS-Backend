using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Nest;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface for CRUD functionality for Rating entity.
/// </summary>
public class RatingService : IRatingService
{
    private readonly IRatingRepository ratingRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IProviderRepository providerRepository;
    private readonly IParentRepository parentRepository;
    private readonly ILogger<RatingService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly int roundToDigits = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="RatingService"/> class.
    /// </summary>
    /// <param name="ratingRepository">Repository for Rating entity.</param>
    /// <param name="workshopRepository">Repository for Workshop entity.</param>
    /// <param name="providerRepository">Repository for Provider entity.</param>
    /// <param name="parentRepository">Repository for Parent entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public RatingService(
        IRatingRepository ratingRepository,
        IWorkshopRepository workshopRepository,
        IProviderRepository providerRepository,
        IParentRepository parentRepository,
        ILogger<RatingService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
        this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        this.parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<bool> IsReviewed(Guid parentId, Guid workshopId)
    {
        return await ratingRepository.Any(rating => rating.ParentId == parentId
            && rating.Type == RatingType.Workshop
            && rating.EntityId == workshopId).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RatingDto>> GetAsync(OffsetFilter filter)
    {
        logger.LogInformation("Getting part of Ratings started.");

        var ratings = ratingRepository.Get(filter.From, filter.Size, "Parent");

        logger.LogInformation(!ratings.Any()
            ? "Rating table is empty."
            : $"All {ratings.Count()} records were successfully received from the Rating table");

        var ratingsDto = ratings.Select(r => mapper.Map<RatingDto>(r));

        return await AddParentInfoAsync(ratingsDto).ConfigureAwait(false);
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

        return mapper.Map<RatingDto>(rating);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RatingDto>> GetAllByEntityId(Guid entityId, RatingType type, OffsetFilter filter)
    {
        logger.LogInformation($"Getting all Ratings with EntityId = {entityId} and RatingType = {type} started.");

        var ratings = await ratingRepository
            .Get(filter.From, filter.Size, where: r => r.EntityId == entityId && r.Type == type)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(ratings.IsNullOrEmpty()
            ? "Rating table is empty."
            : $"All {ratings.Count()} records with EntityId = {entityId} and RatingType = {type} " +
              $"were successfully received from the Rating table");

        var ratingsDto = ratings.Select(r => mapper.Map<RatingDto>(r));

        return await AddParentInfoAsync(ratingsDto).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RatingDto>> GetAllWorshopsRatingByProvider(Guid id)
    {
        logger.LogInformation($"Getting all Worshops Ratings by ProviderId = {id} started.");

        var worshops = await workshopRepository.GetByFilter(x => x.Provider.Id == id).ConfigureAwait(false);

        List<Rating> worshopsRating = new List<Rating>();

        foreach (var workshop in worshops)
        {
            worshopsRating.AddRange(await ratingRepository.GetByFilter(r => r.EntityId == workshop.Id
                                                                            && r.Type == RatingType.Workshop).ConfigureAwait(false));
        }

        logger.LogInformation(!worshopsRating.Any()
            ? "Rating table is empty."
            : $"All {worshopsRating.Count} records with ProviderId = {id} " +
              $"were successfully received from the Rating table");

        var ratingsDto = worshopsRating.Select(r => mapper.Map<RatingDto>(r));

        return await AddParentInfoAsync(ratingsDto).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RatingDto> GetParentRating(Guid parentId, Guid entityId, RatingType type)
    {
        logger.LogInformation($"Getting Rating for Parent started. Looking parentId = {parentId}, entityId = {entityId} and type = {type}.");

        var rating = (await ratingRepository
            .GetByFilter(r => r.ParentId == parentId
                              && r.EntityId == entityId
                              && r.Type == type)
            .ConfigureAwait(false)).FirstOrDefault();

        logger.LogInformation($"Successfully got a Rating for Parent with Id = {parentId}");

        return rating is null ? null : mapper.Map<RatingDto>(rating);
    }

    /// <inheritdoc/>
    public Tuple<float, int> GetAverageRating(Guid entityId, RatingType type)
    {
        var ratingTuple = ratingRepository.GetAverageRating(entityId, type);
        return new Tuple<float, int>((float)Math.Round(ratingTuple?.Item1 ?? default, roundToDigits), ratingTuple?.Item2 ?? default);
    }

    /// <inheritdoc/>
    public Dictionary<Guid, Tuple<float, int>> GetAverageRatingForRange(IEnumerable<Guid> entities, RatingType type)
    {
        var entitiesRating = ratingRepository.GetAverageRatingForEntities(entities, type);

        var formattedEntities = new Dictionary<Guid, Tuple<float, int>>(entitiesRating.Count);

        foreach (var entity in entitiesRating)
        {
            formattedEntities.Add(entity.Key, new Tuple<float, int>((float)Math.Round(entity.Value.Item1, roundToDigits), entity.Value.Item2));
        }

        return formattedEntities;
    }

    /// <inheritdoc/>
    public async Task<Tuple<float, int>> GetAverageRatingForProvider(Guid providerId)
    {
        var workshops = await workshopRepository.GetByFilter(workshop => workshop.ProviderId == providerId).ConfigureAwait(false);
        var workshopIds = workshops.Select(w => w.Id);
        var workshopAverageRatings = GetAverageRatingForRange(workshopIds, RatingType.Workshop);

        if (workshopAverageRatings.Count() < 1)
        {
            return Tuple.Create<float, int>(0, 0);
        }

        return Tuple.Create<float, int>((float)Math.Round(workshopAverageRatings.Values.Average(r => r.Item1), roundToDigits), workshopAverageRatings.Values.Sum(r => r.Item2));
    }

    /// <inheritdoc/>
    public async Task<Dictionary<Guid, Tuple<float, int>>> GetAverageRatingForProviders(IEnumerable<Guid> providerIds)
    {
        var providers = new Dictionary<Guid, Tuple<float, int>>();
        foreach (var providerId in providerIds)
        {
            providers.Add(providerId, await GetAverageRatingForProvider(providerId).ConfigureAwait(false));
        }

        return providers;
    }

    /// <inheritdoc/>
    public async Task<RatingDto> Create(RatingDto dto)
    {
        logger.LogInformation("Rating creating was started.");

        if (await CheckRatingCreation(dto).ConfigureAwait(false))
        {
            var rating = await ratingRepository.Create(mapper.Map<Rating>(dto)).ConfigureAwait(false);

            logger.LogInformation($"Rating with Id = {rating?.Id} created successfully.");

            return mapper.Map<RatingDto>(rating);
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
            var rating = await ratingRepository.Update(mapper.Map<Rating>(dto)).ConfigureAwait(false);

            logger.LogInformation($"Rating with Id = {rating?.Id} updated succesfully.");

            return mapper.Map<RatingDto>(rating);
        }

        logger.LogInformation("Rating doesn't exist or couldn't change EntityId, Parent and Type.");

        return null;
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting Rating with Id = {id} started.");

        var rating = await ratingRepository.GetById(id).ConfigureAwait(false);

        if (rating == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Rating with Id = {id} doesn't exist in the system"]);
        }

        await ratingRepository.Delete(rating).ConfigureAwait(false);

        logger.LogInformation($"Rating with Id = {id} succesfully deleted.");
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

        if (!await EntityExists(dto.EntityId, dto.Type).ConfigureAwait(false))
        {
            logger.LogInformation($"Record with entityId { dto.EntityId } " +
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
            logger.LogInformation($"Record with entityId { dto.EntityId } " +
                                  $"and Type { dto.Type } don't exist in the system.");

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
    private async Task<bool> EntityExists(Guid id, RatingType type)
    {
        switch (type)
        {
            case RatingType.Provider:
                Provider provider = providerRepository.GetByFilterNoTracking(x => x.Id == id).FirstOrDefault();
                return provider != null;

            case RatingType.Workshop:
                Workshop workshop = workshopRepository.GetByFilterNoTracking(x => x.Id == id).FirstOrDefault();
                return workshop != null;

            default:
                return false;
        }
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