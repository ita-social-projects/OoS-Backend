using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using OutOfSchool.ElasticsearchData.Extensions;

namespace OutOfSchool.BusinessLogic.Config.Elasticsearch;

/// <summary>
/// Contains methods to configure <see cref="WorkshopES"/> model into Elasticsearch index.
/// </summary>
public class ElasticsearchWorkshopConfiguration : IElasticsearchEntityTypeConfiguration<WorkshopES>
{
    private const string DefaultLanguage = "uk";
    private const string DefaultCountry = "UA";
    private const string TimeFormat = "HH:mm:ss";

    /// <inheritdoc/>
    public Action<CreateIndexRequestDescriptor<WorkshopES>> Configure()
    {
        return descriptor => descriptor
            .Mappings(map => map
                .Properties(p => p
                    .Object(g => g.Address, c => c
                        .Properties(p => p.GeoPoint(g => g.Address.Point)))
                    .Nested(n => n.DateTimeRanges, c => c
                        .Properties(dp => dp
                            .Date(
                                nameof(DateTimeRangeES.StartTime).FirstCharToLowerCase(),
                                d => d.Format(TimeFormat))
                            .Date(
                                nameof(DateTimeRangeES.EndTime).FirstCharToLowerCase(),
                                d => d.Format(TimeFormat))))
                    .Keyword(n => n.DirectionIds)
                    .Keyword(n => n.FormOfLearning)
                    .Keyword(n => n.Id)
                    .Keyword(n => n.InstitutionId)
                    .Keyword(n => n.ProviderStatus)
                    .Keyword(n => n.Status)
                    .Keyword(n => n.Title, i => i
                        .Fields(p => p
                            .Text(WorkshopES.TextSuffix)
                            .IcuCollation(WorkshopES.SortSuffix, ic => ic
                                .Language(DefaultLanguage)
                                .Country(DefaultCountry)
                                .CaseFirst(IcuCollationCaseFirst.Upper))))
                    .Keyword(n => n.AgeComposition)
                    .Keyword(n => n.EducationalShift)
                    .Boolean(n => n.ShortStay)
                    .Boolean(n => n.IsSelfFinanced)
                    .Boolean(n => n.IsPaid)
                    .Text(n => n.CompetitiveSelectionDescription)
                    .Text(n => n.DisabilityOptionsDesc)
                    .Boolean(n => n.IsSpecial)
                    .Keyword(n => n.SpecialNeedsType)
                    .Boolean(n => n.IsInclusive)
                    .Text(n => n.EnrollmentProcedureDescription)
                    .Boolean(n => n.AreThereBenefits)
                    .Text(n => n.PreferentialTermsOfParticipation)
                    .Keyword(n => n.Coverage)
                    .Keyword(n => n.Tags)));
    }
}