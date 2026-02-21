using Elastic.Clients.Elasticsearch;
using OutOfSchool.BusinessLogic.Services.Elasticsearch;

namespace OutOfSchool.BusinessLogic.Services;

public interface IElasticsearchSynchronizationService<TService, TEntity> : IAddNewRecordToESSynchronizationTableService
{
    Func<TService, List<Guid>, Task<IEnumerable<TEntity>>> GetbyIds { get; }    

    Task Synchronize(IndexName indexName, CancellationToken cancellationToken);
}