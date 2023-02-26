using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using Nest;
using NuGet.Packaging;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using System.Collections.Generic;
using System.Drawing.Text;

namespace OutOfSchool.WebApi.Services.AverageRatings;

public class AverageRatingService : IAverageRatingService
{
    private readonly OutOfSchoolDbContext db;
    private readonly ILogger<AverageRatingService> logger;
    private readonly IMapper mapper;
    private readonly IAverageRatingRepository averageRatingRepository;
    private readonly IRatingService ratingService;
    private readonly IWorkshopRepository workshopRepository;

    public AverageRatingService(
        OutOfSchoolDbContext db,
        ILogger<AverageRatingService> logger,
        IMapper mapper,
        IAverageRatingRepository averageRatingRepository,
        IRatingService ratingService,
        IWorkshopRepository workshopRepository)
    {
        this.db = db;
        this.logger = logger;
        this.mapper = mapper;
        this.averageRatingRepository = averageRatingRepository;
        this.ratingService = ratingService;
        this.workshopRepository = workshopRepository;
    }

    /// <inheritdoc/>
    public async Task<AverageRatingDto> GetByEntityIdAsync(Guid entityId)
    {
        logger.LogInformation("Getting the average rating by workshop's or provider's id started.");

        var rating = (await averageRatingRepository.GetByFilter(r => r.EntityId == entityId).ConfigureAwait(false)).SingleOrDefault();

        logger.LogInformation("Getting the average rating by workshop's or provider's id finished.");

        return mapper.Map<AverageRatingDto>(rating);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AverageRatingDto>> GetByEntityIdsAsync(IEnumerable<Guid> entityIds)
    {
        logger.LogInformation("Getting the average ratings by the list of the workshop's or provider's ids started.");

        var ratings = await averageRatingRepository.GetByFilter(r => entityIds.Contains(r.EntityId)).ConfigureAwait(false);

        logger.LogInformation("Getting the average ratings by the list of the workshop's or provider's ids finished.");

        return mapper.Map<IEnumerable<AverageRatingDto>>(ratings);
    }

    /// <inheritdoc/>
    public async Task CalculateAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("The average ratings calculation was started.");

        var lastSuccessRatingsCalculationDate = await GetLastSuccessRatingCalculationDateAsync().ConfigureAwait(false);
        var currentRatingsCalculationDate = DateTimeOffset.UtcNow;

        var newAddedRatings = await ratingService.GetAllAsync(r => r.CreationTime > lastSuccessRatingsCalculationDate).ConfigureAwait(false);

        if (newAddedRatings.Any())
        {
            var averageRatings = await RecalculateAverageRatingsAsync(newAddedRatings).ConfigureAwait(false);

            var executionStrategy = db.Database.CreateExecutionStrategy();
            await executionStrategy.Execute(SaveAverageRatings).ConfigureAwait(false);

            async Task SaveAverageRatings()
            {
                await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    await SaveAverageRatinsgAsync(averageRatings, currentRatingsCalculationDate).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogError("The average ratings calculation error. The average ratings weren't saved.");
                    throw;
                }
            }
        }

        logger.LogInformation("The average ratings calculation was finished.");
    }

    private async Task<DateTimeOffset> GetLastSuccessRatingCalculationDateAsync()
    {
        logger.LogInformation("Getting the date of the last success rating calculation started.");

        var ratings = await averageRatingRepository.GetAll().ConfigureAwait(false);

        logger.LogInformation("Getting the date of the last success rating calculation finished.");

        return ratings.Any() ? ratings.Max(r => r.CreationTime) : DateTimeOffset.MinValue;
    }

    private async Task<Dictionary<Guid, Tuple<float, int>>> RecalculateAverageRatingsAsync(IEnumerable<RatingDto> ratings)
    {
        logger.LogInformation("The average ratings calculation started.");

        var workshopsIds = GetUniqueWorkshopIds(ratings);
        var providersIds = await GetUniqueProviderIdsByWorkshopIdsAsync(workshopsIds).ConfigureAwait(false);

        Dictionary<Guid, Tuple<float, int>> averageRatings = new Dictionary<Guid, Tuple<float, int>>();

        averageRatings.AddRange(await CalculateWorkshopsRatingsByEntityIdsAsync(workshopsIds).ConfigureAwait(false));
        averageRatings.AddRange(await CalculateProvidersRatingsByProvidersIdsAsync(providersIds).ConfigureAwait(false));

        logger.LogInformation("The average ratings calculation finished.");

        return averageRatings;
    }

    private IEnumerable<Guid> GetUniqueWorkshopIds(IEnumerable<RatingDto> ratings)
    {
        return ratings.Select(x => x.EntityId).Distinct();

    }

    private async Task<IEnumerable<Guid>> GetUniqueProviderIdsByWorkshopIdsAsync(IEnumerable<Guid> workshopsIds)
    {
        List<Guid> providersIds = new List<Guid>();

        foreach (var workshopId in workshopsIds)
        {
            var workshop = await workshopRepository
                .GetByFilterNoTracking(w => w.Id == workshopId)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);

            if (workshop != null)
            {
                providersIds.Add(workshop.ProviderId);
            }
        }

        return providersIds.Distinct();
    }

    private async Task<Tuple<float, int>> CalculateWorkshopRatingByEntityIdAsync(Guid workshopId)
    {
        var ratings = await ratingService.GetAllAsync(r => r.EntityId == workshopId).ConfigureAwait(false);

        return ratings
            .GroupBy(r => r.EntityId)
            .Select(g => Tuple.Create((float)g.Average(e => e.Rate), g.Count()))
            .FirstOrDefault();
    }

    private async Task<Dictionary<Guid, Tuple<float, int>>> CalculateWorkshopsRatingsByEntityIdsAsync(IEnumerable<Guid> workshopIds)
    {
        var ratings = new Dictionary<Guid, Tuple<float, int>>();

        foreach (var entityId in workshopIds)
        {
            ratings.Add(entityId, await CalculateWorkshopRatingByEntityIdAsync(entityId).ConfigureAwait(false));
        }

        return ratings;
    }

    private async Task<Tuple<float, int>> CalculateProviderRatingByEntityIdAsync(Guid providerId)
    {
        var workshopsRatings = await ratingService.GetAllWorshopsRatingByProvider(providerId).ConfigureAwait(false);

        return new Tuple<float, int>((float)workshopsRatings.Average(w => w.Rate), workshopsRatings.Count());
    }

    public async Task<Dictionary<Guid, Tuple<float, int>>> CalculateProvidersRatingsByProvidersIdsAsync(IEnumerable<Guid> providerIds)
    {
        var providers = new Dictionary<Guid, Tuple<float, int>>();

        foreach (var providerId in providerIds)
        {
            providers.Add(providerId, await CalculateProviderRatingByEntityIdAsync(providerId).ConfigureAwait(false));
        }

        return providers;
    }

    private async Task SaveAverageRatinsgAsync(Dictionary<Guid, Tuple<float, int>> ratings, DateTimeOffset date)
    {
        logger.LogInformation("Saving the average ratings was started.");

        foreach (var rating in ratings)
        {
            var entity = averageRatingRepository.Get(where: x => x.EntityId == rating.Key).SingleOrDefault();

            if (entity != null)
            {
                entity.Rate = rating.Value.Item1;
                entity.RateQuantity = rating.Value.Item2;
                entity.CreationTime = date;
                await averageRatingRepository.Update(entity).ConfigureAwait(false);
            }
            else
            {
                await averageRatingRepository
                    .Create(
                        new AverageRating()
                        {
                            Rate = rating.Value.Item1,
                            RateQuantity = rating.Value.Item2,
                            EntityId = rating.Key,
                            CreationTime = date,
                        })
                    .ConfigureAwait(false);
            }
        }

        logger.LogInformation("Saving the average ratings was finished.");
    }
}