﻿using AutoMapper;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;

namespace OutOfSchool.WebApi.Services.Strategies.WorkshopStrategies;

public class WorkshopESStrategy : IWorkshopStrategy
{
    private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;
    private readonly ILogger<WorkshopESStrategy> logger;
    private readonly IMapper mapper;

    public WorkshopESStrategy(
        IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService,
        ILogger<WorkshopESStrategy> logger,
        IMapper mapper)
    {
        this.elasticsearchService = elasticsearchService ?? throw new ArgumentNullException(nameof(elasticsearchService));
        this.logger = logger;
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter)
    {
        var result = await elasticsearchService.Search(mapper.Map<WorkshopFilterES>(filter)).ConfigureAwait(false);

        if (result.TotalAmount <= 0)
        {
            logger.LogInformation("Result was {TotalAmount}", result.TotalAmount);
        }

        return mapper.Map<SearchResult<WorkshopCard>>(result);
    }
}
