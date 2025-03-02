using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

public class CompetitiveEventSynchronizationService : ElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent, CompetitiveEventES, CompetitiveEventFilterES>
{
    public CompetitiveEventSynchronizationService(
        ICompetitiveEventService competitiveEventService,
        IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository,
        IElasticsearchProvider<CompetitiveEventES, CompetitiveEventFilterES> esProvider,
        ILogger<CompetitiveEventSynchronizationService> logger,
        IMapper mapper,
        IOptions<ElasticsearchSynchronizationSchedulerConfig> options,
        IAddNewRecordToESSynchronizationTableService addNewRecordToESSynchronizationTableService) :
        base(competitiveEventService, elasticsearchSyncRecordRepository, esProvider, logger, mapper, options, addNewRecordToESSynchronizationTableService)
    {
    }

    public override Func<ICompetitiveEventService, List<Guid>, Task<IEnumerable<CompetitiveEvent>>> GetbyIds
    {
        get
        {
            return (service, ids) => service.GetByIds(ids);
        }
    }
}
