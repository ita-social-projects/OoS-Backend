using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Strategies.WorkshopStrategies;

public class WorkshopESStrategy(
    IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService,
    ILogger<WorkshopESStrategy> logger
) : IWorkshopStrategy
{
    public async Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter)
    {
        var result = await elasticsearchService.Search(filter.ToES()).ConfigureAwait(false);

        if (result.TotalAmount <= 0)
        {
            logger.LogInformation("Result was {TotalAmount}", result.TotalAmount);
        }

        return new SearchResult<WorkshopCard>()
        {
            TotalAmount = result.TotalAmount,
            Entities = result.Entities.ToCard()
        };
    }

    public async Task<PriceRange> GetPriceRangeAsync(WorkshopFilter filter)
    {
        var result = await elasticsearchService.GetPriceRangeAsync(filter.ToES()).ConfigureAwait(false);

        if (result.MinPrice == 0 && result.MaxPrice == 0)
        {
            logger.LogDebug("Result was {MinPrice} - {MaxPrice}", result.MinPrice, result.MaxPrice);
        }

        return result.ToDto();
    }
}
