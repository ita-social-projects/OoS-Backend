using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

public class WorkshopSynchronizationService : ElasticsearchSynchronizationService<IWorkshopService, Workshop, WorkshopES, WorkshopFilterES>
{
    public WorkshopSynchronizationService(
        IWorkshopService workshopService, 
        IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository, 
        IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider, 
        ILogger<WorkshopSynchronizationService> logger, 
        IMapper mapper, 
        IOptions<ElasticsearchSynchronizationSchedulerConfig> options,
        IAddNewRecordToESSynchronizationTableService addNewRecordToESSynchronizationTableService) : 
        base(workshopService, elasticsearchSyncRecordRepository, esProvider, logger, mapper, options, addNewRecordToESSynchronizationTableService)
    {
    }

    public override Func<IWorkshopService, List<Guid>, Task<IEnumerable<Workshop>>> GetbyIds
    {
        get
        {
            return (service, ids) => service.GetByIds(ids);
        }
    }
}
