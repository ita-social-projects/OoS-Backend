using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

public interface IAddNewRecordToESSynchronizationTableService
{
    Task AddNewRecordToElasticsearchSynchronizationTable(
        ElasticsearchSyncEntity entity,
        Guid id,
        ElasticsearchSyncOperation operation);
}
