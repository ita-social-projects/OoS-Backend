﻿using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Strategies.WorkshopStrategies;

public class WorkshopServiceStrategy : IWorkshopStrategy
{
    private readonly IWorkshopService workshopService;
    private readonly ILogger<WorkshopServiceStrategy> logger;

    public WorkshopServiceStrategy(IWorkshopService workshopService, ILogger<WorkshopServiceStrategy> logger)
    {
        this.workshopService = workshopService ?? throw new ArgumentNullException();
        this.logger = logger;
    }

    public async Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter)
    {
        filter ??= new WorkshopFilter();

        var databaseResult = filter.OrderByField == nameof(OrderBy.Nearest)
            ? await workshopService.GetNearestByFilter(filter).ConfigureAwait(false)
            : await workshopService.GetByFilter(filter).ConfigureAwait(false);

        if (databaseResult.TotalAmount <= 0)
        {
            logger?.LogInformation($"Result was {databaseResult.TotalAmount}");
        }

        return new SearchResult<WorkshopCard>() { TotalAmount = databaseResult.TotalAmount, Entities = databaseResult.Entities };
    }
}