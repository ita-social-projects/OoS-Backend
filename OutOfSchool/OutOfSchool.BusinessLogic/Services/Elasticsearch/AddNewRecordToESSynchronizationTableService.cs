using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;
public class AddNewRecordToESSynchronizationTableService : IAddNewRecordToESSynchronizationTableService
{
    private readonly IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository;
    private readonly ILogger<AddNewRecordToESSynchronizationTableService> logger;

    public AddNewRecordToESSynchronizationTableService(
        IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository, 
        ILogger<AddNewRecordToESSynchronizationTableService> logger)
    {
        this.elasticsearchSyncRecordRepository = elasticsearchSyncRecordRepository;
        this.logger = logger;
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
}
