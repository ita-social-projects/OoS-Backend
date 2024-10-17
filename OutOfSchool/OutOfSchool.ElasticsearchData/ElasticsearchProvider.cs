using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.ElasticsearchData;

/// <summary>
/// A provider to the Elasticsearch API.
/// </summary>
/// <typeparam name="TEntity">The type of entity that will be used for strongly typed queries.</typeparam>
/// <typeparam name="TSearch">The type of filter for searching purposes.</typeparam>
public abstract class ElasticsearchProvider<TEntity, TSearch> : IElasticsearchProvider<TEntity, TSearch>
    where TEntity : class, new()
    where TSearch : class, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticsearchProvider{TEntity, TSearch}"/> class.
    /// </summary>
    /// <param name="elasticClient">The configured instance of Elasticsearch client.</param>
    protected ElasticsearchProvider(ElasticsearchClient elasticClient)
    {
        this.ElasticClient = elasticClient;
    }

    protected ElasticsearchClient ElasticClient { get; private set; }

    /// <inheritdoc/>
    public virtual async Task<Result> IndexEntityAsync(TEntity entity)
    {
        var resp = await ElasticClient.IndexAsync(entity);

        return resp.Result;
    }

    /// <inheritdoc/>
    public virtual async Task<Result> UpdateEntityAsync(TEntity entity)
    {
        var resp = await ElasticClient.UpdateAsync<TEntity, TEntity>(entity, entity, u => u.Doc(entity).Upsert(entity));

        return resp.Result;
    }

    /// <inheritdoc/>
    public virtual async Task<Result> DeleteEntityAsync(TEntity entity)
    {
        var resp = await ElasticClient.DeleteAsync<TEntity>(new DeleteRequestDescriptor<TEntity>(entity));

        return resp.Result;
    }

    public virtual async Task<Result> DeleteRangeOfEntitiesByIdsAsync(IEnumerable<Guid> ids)
    {
        var bulkResponse = await ElasticClient.BulkAsync(new BulkRequest
        {
            Operations = ids.Select(x => new BulkDeleteOperation<TEntity>(x)).Cast<IBulkOperation>().ToList(),
        });

        return Result.Deleted;
    }

    /// <inheritdoc/>
    public virtual async Task<Result> ReIndexAll(IEnumerable<TEntity> source)
    {
        var descriptor = new DeleteByQueryRequestDescriptor<TEntity>(Indices.All)
            .Query(q => q.MatchAll(m => m.Boost(1)));
        await ElasticClient.DeleteByQueryAsync<TEntity>(descriptor).ConfigureAwait(false);
        var result = IndexAll(source);
        return result;
    }

    /// <inheritdoc/>
    public virtual Result IndexAll(IEnumerable<TEntity> source)
    {
        var bulkAllObservable = ElasticClient.BulkAll<TEntity>(source, b => b
            .MaxDegreeOfParallelism(4)
            .BackOffTime("10s")
            .BackOffRetries(2)
            .RefreshOnCompleted()
            .Size(1000));

        var waitHandle = new ManualResetEvent(false);
        ExceptionDispatchInfo exceptionDispatchInfo = null;
        Result result = Result.NoOp;

        var observer = new BulkAllObserver(
            onError: exception =>
            {
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                waitHandle.Set();
            },
            onCompleted: () =>
            {
                result = Result.Updated;
                waitHandle.Set();
            });

        bulkAllObservable.Subscribe(observer);

        waitHandle.WaitOne();

        exceptionDispatchInfo?.Throw();

        return result;
    }

    /// <inheritdoc/>
    public abstract Task<SearchResultES<TEntity>> Search(TSearch filter = null);

    /// <inheritdoc/>
    public async Task<Result> PartialUpdateEntityAsync<TKey>(TKey entityId, IPartial<TEntity> partial)
    {
        // It's important to convert id to string because in other case it recognises as object type
        // and thows exception
        var request = new UpdateRequestDescriptor<TEntity, object>(new Id(entityId.ToString()))
             .Doc(partial);
        var result = await ElasticClient.UpdateAsync(request);

        return result.Result;
    }
}