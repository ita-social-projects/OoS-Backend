using AutoMapper;
using NuGet.Packaging;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Common.QuartzConstants;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.AverageRatings;

public class AverageRatingService : IAverageRatingService
{
    private readonly ILogger<AverageRatingService> logger;
    private readonly IMapper mapper;
    private readonly IEntityRepositorySoftDeleted<long, AverageRating> averageRatingRepository;
    private readonly IRatingService ratingService;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IEntityRepositorySoftDeleted<long, Rating> ratingRepository;
    private readonly IEntityRepository<long, QuartzJob> quartzJobRepository;
    private readonly IOperationWithObjectService operationWithObjectService;

    public AverageRatingService(
        ILogger<AverageRatingService> logger,
        IMapper mapper,
        IEntityRepositorySoftDeleted<long, AverageRating> averageRatingRepository,
        IRatingService ratingService,
        IWorkshopRepository workshopRepository,
        IEntityRepositorySoftDeleted<long, Rating> ratingRepository,
        IEntityRepository<long, QuartzJob> quartzJobRepository,
        IOperationWithObjectService operationWithObjectService)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.averageRatingRepository = averageRatingRepository;
        this.ratingService = ratingService;
        this.workshopRepository = workshopRepository;
        this.ratingRepository = ratingRepository;
        this.quartzJobRepository = quartzJobRepository;
        this.operationWithObjectService = operationWithObjectService;
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

        var lastSuccessQuartzJobLaunchDate = await GetLastSuccessQuartzJobLaunch().ConfigureAwait(false);

        var currentRatingsCalculationDate = DateTimeOffset.UtcNow;

        var newAddedRatings = await GetNewRatings(lastSuccessQuartzJobLaunchDate).ConfigureAwait(false);

        if (newAddedRatings.Any() || await RatingsWereDeleted())
        {
            var averageRatings = await RecalculateAverageRatingsAsync(newAddedRatings).ConfigureAwait(false);

            try
            {
                await averageRatingRepository.RunInTransaction(SaveAverageRatings).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError("The average ratings calculation error. The average ratings weren't saved.");
                throw;
            }

            async Task SaveAverageRatings()
            {
                await SaveAverageRatinsgAsync(averageRatings).ConfigureAwait(false);
            }

            await SaveLastSuccessQuartzJobLaunch(currentRatingsCalculationDate).ConfigureAwait(false);
        }

        logger.LogInformation("The average ratings calculation was finished.");
    }

    private async Task<DateTimeOffset> GetLastSuccessQuartzJobLaunch()
    {
        var job = quartzJobRepository.Get(whereExpression: x => x.Name == JobConstants.AverageRatingCalculating).SingleOrDefault();

        return job?.LastSuccessLaunch ?? DateTimeOffset.MinValue;
    }

    private async Task SaveLastSuccessQuartzJobLaunch(DateTimeOffset date)
    {
        var job = quartzJobRepository.Get(whereExpression: x => x.Name == JobConstants.AverageRatingCalculating).SingleOrDefault();

        if (job is not null)
        {
            job.LastSuccessLaunch = date;
            _ = await quartzJobRepository.Update(job).ConfigureAwait(false);
        }
        else
        {
            job = new QuartzJob() { Name = JobConstants.AverageRatingCalculating, LastSuccessLaunch = date };
            _ = await quartzJobRepository.Create(job).ConfigureAwait(false);
        }
    }

    private async Task<IEnumerable<RatingDto>> GetNewRatings(DateTimeOffset lastLaunchDate)
    {
        return await ratingService.GetAllAsync(r => r.CreationTime > lastLaunchDate).ConfigureAwait(false);
    }

    private async Task<Dictionary<Guid, Tuple<float, int>>> RecalculateAverageRatingsAsync(IEnumerable<RatingDto> ratings)
    {
        logger.LogInformation("The average ratings calculation started.");

        var filter = new OperationWithObjectFilter() { OperationType = OperationWithObjectOperationType.RecalculateAverageRating };
        var operations = await operationWithObjectService.GetAll(filter);
        var workshopsIdsFromOperations = operations.Select(x => (Guid)x.EntityId);

        var workshopsIds = GetUniqueWorkshopIds(ratings);
        workshopsIds = workshopsIds.Union(workshopsIdsFromOperations);

        var providersIds = GetUniqueProviderIdsByWorkshopIdsAsync(workshopsIds);

        Dictionary<Guid, Tuple<float, int>> averageRatings = new Dictionary<Guid, Tuple<float, int>>();

        var averageWorkshopsRatings = CalculateWorkshopsRatingsByEntityIdsAsync(workshopsIds);
        averageRatings.AddRange(averageWorkshopsRatings);

        var averageProvidersRatings = CalculateProvidersRatingsByProvidersIdsAsync(providersIds);
        averageRatings.AddRange(averageProvidersRatings);

        await averageRatingRepository.RunInTransaction(DeleteAverageRatings).ConfigureAwait(false);

        foreach (var operation in operations)
        {
            await operationWithObjectService.Delete(operation.Id);
        }

        async Task DeleteAverageRatings()
        {
            await DeleteAverageRatingsForWorkshopsWithoutRatings(workshopsIdsFromOperations, averageWorkshopsRatings.Keys).ConfigureAwait(false);
            await DeleteAverageRatingsForProvidersWithoutRatings(workshopsIdsFromOperations, averageProvidersRatings.Keys).ConfigureAwait(false);
        }

        logger.LogInformation("The average ratings calculation finished.");

        return averageRatings;
    }

    private IEnumerable<Guid> GetUniqueWorkshopIds(IEnumerable<RatingDto> ratings)
    {
        return ratings.Select(x => x.EntityId).Distinct();
    }

    private IEnumerable<Guid> GetUniqueProviderIdsByWorkshopIdsAsync(IEnumerable<Guid> workshopsIds)
    {
        return workshopRepository
            .GetByFilterNoTracking(w => workshopsIds.Contains(w.Id))
            .Select(w => w.ProviderId)
            .Distinct();
    }

    private Dictionary<Guid, Tuple<float, int>> CalculateWorkshopsRatingsByEntityIdsAsync(IEnumerable<Guid> workshopIds)
    {
        return ratingRepository.GetByFilterNoTracking(r => workshopIds.Contains(r.EntityId))
            .AsEnumerable()
            .GroupBy(r => r.EntityId)
            .ToDictionary(g => g.Key, g => Tuple.Create((float)g.Average(r => r.Rate), g.Count()));
    }

    private Dictionary<Guid, Tuple<float, int>> CalculateProvidersRatingsByProvidersIdsAsync(IEnumerable<Guid> providerIds)
    {
        var workshops = workshopRepository.GetByFilterNoTracking(x => providerIds.Contains(x.Provider.Id));

        var workshopsIds = workshops.Select(x => x.Id);

        var providersWorkshops = workshops.Select(w => new { w.ProviderId, WorkshopId = w.Id, }).AsEnumerable();

        var workshopsRatings = ratingRepository
            .GetByFilterNoTracking(x => workshopsIds.Contains(x.EntityId))
            .Select(r => new { WorkshopId = r.EntityId, r.Rate, })
            .AsEnumerable();

        var providersRatings = providersWorkshops
            .Join(workshopsRatings, w => w.WorkshopId, r => r.WorkshopId, (w, r) => new { w.ProviderId, r.Rate })
            .GroupBy(x => x.ProviderId)
            .ToDictionary(g => g.Key, g => new Tuple<float, int>((float)g.Average(x => x.Rate), g.Count()));

        return providersRatings;
    }

    private async Task SaveAverageRatinsgAsync(Dictionary<Guid, Tuple<float, int>> ratings)
    {
        logger.LogInformation("Saving the average ratings was started.");

        foreach (var rating in ratings)
        {
            var entity = averageRatingRepository.Get(whereExpression: x => x.EntityId == rating.Key).SingleOrDefault();

            if (entity != null)
            {
                entity.Rate = rating.Value.Item1;
                entity.RateQuantity = rating.Value.Item2;
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
                        })
                    .ConfigureAwait(false);
            }
        }

        logger.LogInformation("Saving the average ratings was finished.");
    }

    private async Task<bool> RatingsWereDeleted()
    {
        var filter = new OperationWithObjectFilter()
        {
            OperationType = OperationWithObjectOperationType.RecalculateAverageRating,
        };

        return await operationWithObjectService.Exists(filter);
    }

    private async Task DeleteAverageRatingsForWorkshopsWithoutRatings(IEnumerable<Guid> workshopsIdsFromOperations, IEnumerable<Guid> workshopsIdsWithAverageRatings)
    {
        var workshopsIdsWithoutRatings = workshopsIdsFromOperations.Except(workshopsIdsWithAverageRatings);

        var averageRatingsToDelete = await averageRatingRepository
            .GetByFilter(ar => workshopsIdsWithoutRatings.Contains(ar.EntityId))
            .ConfigureAwait(false);

        foreach (var rating in averageRatingsToDelete)
        {
            await averageRatingRepository.Delete(rating);
        }
    }

    private async Task DeleteAverageRatingsForProvidersWithoutRatings(IEnumerable<Guid> workshopsIdsFromOperations, IEnumerable<Guid> providersIdsWithAverageRatings)
    {
        var providersIds = GetUniqueProviderIdsByWorkshopIdsAsync(workshopsIdsFromOperations);

        var providersIdsWithoutRatings = providersIds.Except(providersIdsWithAverageRatings);

        var averageRatingsToDelete = await averageRatingRepository
            .GetByFilter(ar => providersIdsWithoutRatings.Contains(ar.EntityId))
            .ConfigureAwait(false);

        foreach (var rating in averageRatingsToDelete)
        {
            await averageRatingRepository.Delete(rating);
        }
    }
}
