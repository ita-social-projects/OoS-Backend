using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.AverageRatings;

namespace OutOfSchool.WebApi.Services;

/// <inheritdoc/>
public class ESWorkshopService : IElasticsearchService<WorkshopES, WorkshopFilterES>
{
    private readonly IWorkshopService workshopService;
    private readonly IRatingService ratingService;
    private readonly IAverageRatingService averageRatingService;
    private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;
    private readonly ElasticPinger esPinger;
    private readonly ILogger<ESWorkshopService> logger;

    /// <inheritdoc/>
    public bool IsElasticAlive => esPinger.IsHealthy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ESWorkshopService"/> class.
    /// </summary>
    /// <param name="workshopService">Service that provides access to Workshops in the database.</param>    
    /// <param name="esProvider">Provider to the Elasticsearch workshops index.</param>
    /// <param name="elasticPinger">Background worker pings the Elasticsearch.</param>
    /// <param name="logger">Logger.</param>
    /// /// <param name="averageRatingService">Service that provides access to average ratings in the database.</param>
    public ESWorkshopService(
        IWorkshopService workshopService,
        IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
        ElasticPinger elasticPinger,
        ILogger<ESWorkshopService> logger,
        IAverageRatingService averageRatingService)
    {
        this.workshopService = workshopService;        
        this.esProvider = esProvider;
        this.esPinger = elasticPinger;
        this.logger = logger;
        this.averageRatingService = averageRatingService;
    }

    /// <inheritdoc/>
    public async Task<bool> Index(WorkshopES entity)
    {
        NullCheck(entity);

        try
        {
            var resp = await esProvider.IndexEntityAsync(entity).ConfigureAwait(false);

            if (resp == Result.Error)
            {
                return false;
            }
        }
        catch (ElasticsearchClientException)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> Update(WorkshopES entity)
    {
        NullCheck(entity);

        try
        {
            entity.Rating = (await averageRatingService.GetByEntityIdAsync(entity.Id).ConfigureAwait(false)).Rate;

            var resp = await esProvider.UpdateEntityAsync(entity).ConfigureAwait(false);

            if (resp == Result.Error)
            {
                return false;
            }
        }
        catch (ElasticsearchClientException)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> Delete(Guid id)
    {
        try
        {
            var resp = await esProvider.DeleteEntityAsync(new WorkshopES() { Id = id }).ConfigureAwait(false);

            if (resp == Result.Error)
            {
                return false;
            }
        }
        catch (ElasticsearchClientException)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ReIndex()
    {
        try
        {
            var filter = new OffsetFilter() { From = 0, Size = 500 };
            var source = new List<WorkshopES>();

            var data = await workshopService.GetAll(filter).ConfigureAwait(false);
            while (data.Entities.Count > 0)
            {
                foreach (var entity in data.Entities)
                {
                    entity.Rating = (await averageRatingService.GetByEntityIdAsync(entity.Id).ConfigureAwait(false)).Rate;
                    source.Add(entity.ToESModel());
                }

                filter.From += filter.Size;
                data = await workshopService.GetAll(filter).ConfigureAwait(false);
            }

            var resp = await esProvider.ReIndexAll(source).ConfigureAwait(false);

            if (resp == Result.Error)
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<SearchResultES<WorkshopES>> Search(WorkshopFilterES filter)
    {
        try
        {
            var res = await esProvider.Search(filter).ConfigureAwait(false);

            return res;
        }
        catch
        {
            return new SearchResultES<WorkshopES>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> PartialUpdate(Guid id, IPartial<WorkshopES> partialWorkshop)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            var responce = await esProvider.PartialUpdateEntityAsync(id, partialWorkshop).ConfigureAwait(false);

            if (responce == Result.Error)
            {
                return false;
            }
        }
        catch (ElasticsearchClientException ex)
        {
            logger.LogError(ex, $"Partial update in ElasticSearch failed: {ex.Message}");
            return false;
        }

        return true;
    }

    private void NullCheck(WorkshopES entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException($"{entity} is not set to an instance.");
        }
    }
}