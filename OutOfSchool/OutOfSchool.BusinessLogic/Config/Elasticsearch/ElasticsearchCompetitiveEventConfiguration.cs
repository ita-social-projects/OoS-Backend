using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using OutOfSchool.ElasticsearchData.Extensions;

namespace OutOfSchool.BusinessLogic.Config.Elasticsearch;

/// <summary>
/// Contains methods to configure <see cref="CompetitiveEventES"/> model into Elasticsearch index.
/// </summary>
public class ElasticsearchCompetitiveEventConfiguration : IElasticsearchEntityTypeConfiguration<CompetitiveEventES>
{
    private const string DefaultLanguage = "uk";
    private const string DefaultCountry = "UA";

    /// <inheritdoc/>
    public Action<CreateIndexRequestDescriptor<CompetitiveEventES>> Configure()
    {
        return descriptor => descriptor
           .Mappings(map => map
               .Properties(p => p
                   .Keyword(n => n.Title, i => i
                        .Fields(p => p
                            .Text(CompetitiveEventES.TextSuffix)
                            .IcuCollation(CompetitiveEventES.SortSuffix, ic => ic
                                .Language(DefaultLanguage)
                                .Country(DefaultCountry)
                                .CaseFirst(IcuCollationCaseFirst.Upper))))
                   .Text(n => n.ShortTitle)
                   .Keyword(n => n.State)
                   .Date(nameof(CompetitiveEventES.RegistrationStartTime).FirstCharToLowerCase())
                   .Date(nameof(CompetitiveEventES.RegistrationEndTime).FirstCharToLowerCase())
                   .Text(n => n.CompetitiveEventDescriptionItems)
                   .Text(n => n.AdditionalDescription)
                   .Date(nameof(CompetitiveEventES.ScheduledStartTime).FirstCharToLowerCase())
                   .Date(nameof(CompetitiveEventES.ScheduledEndTime).FirstCharToLowerCase())
                   .UnsignedLongNumber(n => n.NumberOfSeats)
                   .Keyword(n => n.CompetitiveEventAccountingTypeId)
                   .Text(n => n.CompetitiveEventAccountingType)
                   .Text(n => n.DescriptionOfTheEnrollmentProcedure)
                   .Keyword(n => n.OrganizerOfTheEventId)
                   .Keyword(n => n.PlannedFormatOfClasses)
                   .Text(n => n.VenueName)
                   .Text(n => n.TermsOfParticipation)
                   .Text(n => n.PreferentialTermsOfParticipation)
                   .Boolean(n => n.AreThereBenefits)
                   .Text(n => n.Benefits)
                   .IntegerNumber(n => n.MinimumAge)
                   .IntegerNumber(n => n.MaximumAge)
                   .Text(n => n.Coverage)
                   .IntegerNumber(n => n.Price)
                   .Boolean(n => n.CompetitiveSelection)
                   .UnsignedLongNumber(n => n.NumberOfOccupiedSeats)));
        }
}
