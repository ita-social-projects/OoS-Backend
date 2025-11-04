using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

public class WorkshopSynchronizationService(
    IWorkshopService workshopService,
    IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository,
    IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
    ILogger<WorkshopSynchronizationService> logger,
    IOptions<ElasticsearchSynchronizationSchedulerConfig> options,
    IAddNewRecordToESSynchronizationTableService addNewRecordToESSynchronizationTableService
) : ElasticsearchSynchronizationService<IWorkshopService, Workshop, WorkshopES, WorkshopFilterES>(
    workshopService, 
    elasticsearchSyncRecordRepository, 
    esProvider, 
    logger,
    workshops => workshops.ToES(), 
    options, 
    addNewRecordToESSynchronizationTableService
)
{
    public override Func<IWorkshopService, List<Guid>, Task<IEnumerable<Workshop>>> GetbyIds 
        => (service, ids) => service.GetByIdsWithIncludes(
            ids,
            query =>
                query
                    .Include(w => w.Teachers)
                    .Include(w => w.DateTimeRanges)
                    .Include(w => w.WorkshopDescriptionItems)
                    .IncludeContactsWithCodeficatorHierarchy()
                    .Include(w => w.LanguageOfEducation)
                    .Include(w => w.Images)
                    .Include(w => w.Provider)
                    .Include(w => w.InstitutionHierarchy)
                    .ThenInclude(ih => ih.Institution)
                    .Include(w => w.InstitutionHierarchy)
                    .ThenInclude(ih => ih.SubDirections)
                    .ThenInclude(sd => sd.Direction));
}
