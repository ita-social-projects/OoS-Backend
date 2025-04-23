using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Services.Elasticsearch;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the operations for synchronization databases.
/// </summary>
public abstract class ElasticsearchSynchronizationService<TService, TEntity, TEntityES, TEntityFilterES>(
    TService databaseService,
    IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository,
    IElasticsearchProvider<TEntityES, TEntityFilterES> esProvider,
    ILogger<ElasticsearchSynchronizationService<TService, TEntity, TEntityES, TEntityFilterES>> logger,
    Func<IEnumerable<TEntity>, List<TEntityES>> mapper,
    IOptions<ElasticsearchSynchronizationSchedulerConfig> options,
    IAddNewRecordToESSynchronizationTableService addNewRecordToESSynchronizationTableService
) : IElasticsearchSynchronizationService<TService, TEntity>
    where TEntityES : class, new()
    where TEntityFilterES : class, new()
{
    public abstract Func<TService, List<Guid>, Task<IEnumerable<TEntity>>> GetbyIds { get; }

    public async Task AddNewRecordToElasticsearchSynchronizationTable(
        ElasticsearchSyncEntity entity,
        Guid id,
        ElasticsearchSyncOperation operation
    ) 
        => await addNewRecordToESSynchronizationTableService.AddNewRecordToElasticsearchSynchronizationTable(entity, id, operation);
    
    public async Task Synchronize(IndexName indexName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Elasticsearch synchronization started");

        try
        {
            var result = await DoSynchronization(indexName).WaitAsync(cancellationToken);
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

    private async Task<bool> DoSynchronization(IndexName indexName)
    {
        var syncEntityName = typeof(TEntity).Name;

        var isValidSyncEnity = Enum.TryParse<ElasticsearchSyncEntity>(syncEntityName, out var syncEntityEnum);

        if (!isValidSyncEnity)
        {
            logger.LogError(
                "Synchronization of Elasticsearch has failed because synchronization entity is invalid. Synchronization Entity = {SyncEntity}",
                syncEntityName);
            return false;
        }

        var elasticsearchSyncRecords = (await elasticsearchSyncRecordRepository.GetByEntity(
            syncEntityEnum,
            options.Value.OperationsPerTask)).ToList();

        var resultCreate =
            await SynchronizeAndDeleteRecords(elasticsearchSyncRecords, ElasticsearchSyncOperation.Create, indexName)
                .ConfigureAwait(false);
        if (!resultCreate)
        {
            logger.LogError(
                "Synchronization of Elasticsearch has failed during {Create} operation",
                ElasticsearchSyncOperation.Create);
            return false;
        }

        var resultUpdate =
            await SynchronizeAndDeleteRecords(elasticsearchSyncRecords, ElasticsearchSyncOperation.Update, indexName)
                .ConfigureAwait(false);
        if (!resultUpdate)
        {
            logger.LogError(
                "Synchronization of Elasticsearch has failed during {Update} operation",
                ElasticsearchSyncOperation.Update);
            return false;
        }

        var resultDelete =
            await SynchronizeAndDeleteRecords(elasticsearchSyncRecords, ElasticsearchSyncOperation.Delete, indexName)
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
        ElasticsearchSyncOperation elasticsearchSyncOperation,
        IndexName indexName)
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

                if (result != Result.Deleted)
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
            var entities = await GetbyIds.Invoke(databaseService, ids);

            var source = mapper(entities);

            try
            {
                var result = esProvider.IndexAll(source, indexName);

                if (result != Result.Updated)
                {
                    logger.LogError("Error happened while trying to update indexes in Elasticsearch.");
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
        ElasticsearchSyncOperation elasticsearchSyncOperation,        
        IndexName indexName)
    {
        try
        {
            var result = await Synchronize(elasticsearchSyncRecords, elasticsearchSyncOperation, indexName)
                .ConfigureAwait(false);

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