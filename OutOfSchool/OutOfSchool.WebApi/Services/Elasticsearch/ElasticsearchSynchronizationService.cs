using AutoMapper;
using Microsoft.Extensions.Options;
using Nest;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the operations for synchronization databases.
/// </summary>
public class ElasticsearchSynchronizationService : IElasticsearchSynchronizationService
{
    private readonly IWorkshopService databaseService;
    private readonly IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository;
    private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;
    private readonly ILogger<ElasticsearchSynchronizationService> logger;
    private readonly IMapper mapper;
    private readonly IOptions<ElasticsearchSynchronizationSchedulerConfig> options;

    public ElasticsearchSynchronizationService(
        IWorkshopService workshopService,
        IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository,
        IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
        ILogger<ElasticsearchSynchronizationService> logger,
        IMapper mapper,
        IOptions<ElasticsearchSynchronizationSchedulerConfig> options)
    {
        this.databaseService = workshopService;
        this.elasticsearchSyncRecordRepository = elasticsearchSyncRecordRepository;
        this.esProvider = esProvider;
        this.logger = logger;
        this.mapper = mapper;
        this.options = options;
    }

    public async Task AddNewRecordToElasticsearchSynchronizationTable(
        ElasticsearchSyncEntity entity,
        Guid id,
        ElasticsearchSyncOperation operation)
    {
        var elasticsearchSyncRecord = new ElasticsearchSyncRecord()
        {
            Entity = entity,
            RecordId = id,
            OperationDate = DateTimeOffset.UtcNow,
            Operation = operation,
        };

        try
        {
            await elasticsearchSyncRecordRepository.Create(elasticsearchSyncRecord).ConfigureAwait(false);

            logger.LogInformation("ElasticsearchSyncRecord created successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Creating new record to ElasticserchSyncRecord failed");
            throw;
        }
    }

    public async Task Synchronize(CancellationToken cancellationToken)
    {
        logger.LogInformation("Elasticsearch synchronization started");

        try
        {
            var result = await DoSynchronization().WaitAsync(cancellationToken);
            if (!result)
            {
                Log.Information("Elasticsearch synchronization failed");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Elasticsearch synchronization failed");
        }

        logger.LogInformation("Elasticsearch synchronization finished");
    }

    private async Task<bool> DoSynchronization()
    {
        var elasticsearchSyncRecords = (await elasticsearchSyncRecordRepository.GetByEntity(
            ElasticsearchSyncEntity.Workshop,
            options.Value.OperationsPerTask)).ToList();

        var resultCreate =
            await SynchronizeAndDeleteRecords(elasticsearchSyncRecords, ElasticsearchSyncOperation.Create)
                .ConfigureAwait(false);
        if (!resultCreate)
        {
            logger.LogError(
                "Synchronization of Elasticsearch has failed during {Create} operation",
                ElasticsearchSyncOperation.Create);
            return false;
        }

        var resultUpdate =
            await SynchronizeAndDeleteRecords(elasticsearchSyncRecords, ElasticsearchSyncOperation.Update)
                .ConfigureAwait(false);
        if (!resultUpdate)
        {
            logger.LogError(
                "Synchronization of Elasticsearch has failed during {Update} operation",
                ElasticsearchSyncOperation.Update);
            return false;
        }

        var resultDelete =
            await SynchronizeAndDeleteRecords(elasticsearchSyncRecords, ElasticsearchSyncOperation.Delete)
                .ConfigureAwait(false);
        if (!resultDelete)
        {
            logger.LogError(
                "Synchronization of Elasticsearch has failed during {Delete} operation",
                ElasticsearchSyncOperation.Delete);
            return false;
        }

        logger.LogInformation("Synchronization of Elasticsearch has finished successfully");

        return true;
    }

    private async Task<bool> Synchronize(
        IEnumerable<ElasticsearchSyncRecord> elasticsearchSyncRecords,
        ElasticsearchSyncOperation elasticsearchSyncOperation)
    {
        var ids = elasticsearchSyncRecords.Where(es => es.Operation == elasticsearchSyncOperation)
            .Select(es => es.RecordId).ToList();

        if (!ids.Any())
        {
            return true;
        }

        if (elasticsearchSyncOperation == ElasticsearchSyncOperation.Delete)
        {
            try
            {
                var result = await esProvider.DeleteRangeOfEntitiesByIdsAsync(ids).ConfigureAwait(false);

                if (result == Result.Error)
                {
                    logger.LogError("Error happened while trying to delete indexes in Elasticsearch");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Delete failed");
                throw;
            }
        }
        else
        {
            var workshops = await databaseService.GetByIds(ids).ConfigureAwait(false);

            var sourse = mapper.Map<List<WorkshopES>>(workshops);

            try
            {
                var result = await esProvider.IndexAll(sourse).ConfigureAwait(false);

                if (result == Result.Error)
                {
                    logger.LogError($"Error happened while trying to update indexes in Elasticsearch.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Operation failed");
                throw;
            }
        }

        return true;
    }

    private async Task<bool> SynchronizeAndDeleteRecords(
        IReadOnlyCollection<ElasticsearchSyncRecord> elasticsearchSyncRecords,
        ElasticsearchSyncOperation elasticsearchSyncOperation)
    {
        try
        {
            var result = await Synchronize(elasticsearchSyncRecords, elasticsearchSyncOperation).ConfigureAwait(false);
            if (result)
            {
                await elasticsearchSyncRecordRepository.DeleteRange(
                    elasticsearchSyncRecords.Where(es => es.Operation == elasticsearchSyncOperation)
                        .Select(es => es.Id)).ConfigureAwait(false);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Delete records in ElasticsearchSyncRecords is failed");
            return false;
        }

        return true;
    }
}