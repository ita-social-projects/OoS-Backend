using Microsoft.Extensions.Options;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

public class CompetitiveEventSynchronizationService(
    ICompetitiveEventService competitiveEventService,
    IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository,
    IElasticsearchProvider<CompetitiveEventES, CompetitiveEventFilterES> esProvider,
    ILogger<CompetitiveEventSynchronizationService> logger,
    IOptions<ElasticsearchSynchronizationSchedulerConfig> options,
    IAddNewRecordToESSynchronizationTableService addNewRecordToESSynchronizationTableService
) : ElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent, CompetitiveEventES, CompetitiveEventFilterES>(
    competitiveEventService, 
    elasticsearchSyncRecordRepository, 
    esProvider, 
    logger,
    CompetitiveEventESExtensions.ToES, 
    options, 
    addNewRecordToESSynchronizationTableService
)
{
    public override Func<ICompetitiveEventService, List<Guid>, Task<IEnumerable<CompetitiveEvent>>> GetbyIds 
        => (service, ids) => service.GetByIds(ids);
}

public static class CompetitiveEventESExtensions
{
    public static CompetitiveEventES ToES(this CompetitiveEvent dto)
        => new()
        { 
            Id = dto.Id,
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            State = dto.State,
            RegistrationStartTime = dto.RegistrationStartTime,
            RegistrationEndTime = dto.RegistrationEndTime,
            CompetitiveEventDescriptionItems = dto.CompetitiveEventDescriptionItems
                .Aggregate(string.Empty, (accumulator, di) =>
                    $"{accumulator}{di.SectionName}{Constants.MappingSeparator}{di.Description}{Constants.MappingSeparator}"),
            AdditionalDescription = dto.AdditionalDescription,
            ScheduledStartTime = dto.ScheduledStartTime,
            ScheduledEndTime = dto.ScheduledEndTime,
            NumberOfSeats = dto.NumberOfSeats,
            CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId,
            CompetitiveEventAccountingType = dto.CompetitiveEventAccountingType.Title,
            DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = dto.OrganizerOfTheEventId,
            PlannedFormatOfClasses = dto.PlannedFormatOfClasses,
            VenueName = dto.VenueName,
            TermsOfParticipation = dto.TermsOfParticipation,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,
            AreThereBenefits = dto.AreThereBenefits,
            Benefits = dto.Benefits,
            MinimumAge = dto.MinimumAge,
            MaximumAge = dto.MaximumAge,
            Coverage = dto.Coverage.Title,
            Price = dto.Price,
            CompetitiveSelection = dto.CompetitiveSelection,
            NumberOfOccupiedSeats = 0,
        };

    public static List<CompetitiveEventES> ToES(this IEnumerable<CompetitiveEvent> list)
        => list.MapToList(ToES);
}
