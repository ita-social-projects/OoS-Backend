using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;

namespace OutOfSchool.WebApi.Services.Strategies;

public class WorkshopStrategy
{
    private IWorkshopStrategy strategy;

    public WorkshopStrategy()
    {

    }

    public WorkshopStrategy(IWorkshopStrategy strategy)
    {
        this.strategy = strategy;
    }

    public void SetStrategy(IWorkshopStrategy strategy)
    {
        this.strategy = strategy;
    }

    public async Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter)
    {
        return await strategy.SearchAsync(filter);
    }
}

public class WorkshopESStrategy : IWorkshopStrategy
{
    private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;
    private readonly ILogger<WorkshopESStrategy> logger;

    public WorkshopESStrategy(IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService, ILogger<WorkshopESStrategy> logger)
    {
        this.elasticsearchService = elasticsearchService;
        this.logger = logger;
    }

    public async Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter)
    {
        var result = await elasticsearchService.Search(filter.ToESModel()).ConfigureAwait(false);

        if (result.TotalAmount <= 0)
        {
            logger.LogInformation($"Result was {result.TotalAmount}");
        }

        return result.ToSearchResult();
    }
}

public class WorkshopServiceStrategy : IWorkshopStrategy
{
    private protected readonly IWorkshopService workshopService;

    public WorkshopServiceStrategy(IWorkshopService workshopService)
    {
        this.workshopService = workshopService;
    }

    public async Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter)
    {
        var databaseResult = await workshopService.GetByFilter(filter).ConfigureAwait(false);
        return new SearchResult<WorkshopCard>() { TotalAmount = databaseResult.TotalAmount, Entities = databaseResult.Entities };
    }
}
