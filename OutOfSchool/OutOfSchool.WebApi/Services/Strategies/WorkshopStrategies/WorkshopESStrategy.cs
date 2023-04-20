using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;

namespace OutOfSchool.WebApi.Services.Strategies.WorkshopStrategies;

public class WorkshopESStrategy : IWorkshopStrategy
{
    private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;
    private readonly ILogger<WorkshopESStrategy> logger;

    public WorkshopESStrategy(IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService, ILogger<WorkshopESStrategy> logger)
    {
        this.elasticsearchService = elasticsearchService ?? throw new ArgumentNullException();
        this.logger = logger;
    }

    public async Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter)
    {
        var result = await elasticsearchService.Search(filter.ToESModel()).ConfigureAwait(false);

        if (result.TotalAmount <= 0)
        {
            logger?.LogInformation($"Result was {result.TotalAmount}");
        }

        return result.ToSearchResult();
    }
}
