using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using OutOfSchool.ElasticsearchData.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.ElasticsearchData;

public class ESCompetitiveEventProvider(ElasticsearchClient elasticClient) :
    ElasticsearchProvider<CompetitiveEventES, CompetitiveEventFilterES>(elasticClient)
{
    public override async Task<SearchResultES<CompetitiveEventES>> Search(CompetitiveEventFilterES filter = null)
    {
        filter ??= new CompetitiveEventFilterES();

        var query = this.CreateQueryFromFilter(filter);
        var request = new SearchRequest<CompetitiveEventES>()
        {
            Query = query,
            From = filter.From,
            Size = filter.Size,
        };

        var resp = await ElasticClient.SearchAsync<CompetitiveEventES>(request);

        return new SearchResultES<CompetitiveEventES>()
        {
            TotalAmount = (int)resp.Total,
            Entities = resp.Documents,
        };
    }

    private Query CreateQueryFromFilter(CompetitiveEventFilterES filter)
    {
        var query = new BoolQuery()
        {
            Filter = [],
            Must = []
        };

        if (filter.Ids.Count != 0)
        {
            query.Filter.Add(new IdsQuery()
            {
                Values = filter.Ids.Select(id => id.ToString()).ToArray(),
            });
            return query;
        }

        AddSearchTextQuery(query, filter);
        AddStatesQuery(query, filter);
        AddPlannedFormatOfClassesQuery(query, filter);
        AddOptionsForPeopleWithDisabilitiesQuery(query, filter);
        AddAreThereBenefitsQuery(query, filter);
        AddCompetitiveSelectionQuery(query, filter);
        AddPriceQuery(query, filter);
        AddAgeQuery(query, filter);
        AddRegistrationEndTimeQuery(query, filter);
        AddScheduledStartTimeQuery(query, filter);

        return query;
    }

    private void AddSearchTextQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            query.Must.Add(new QueryStringQuery()
            {
                Fields = new[]
                {
                    Infer.Field<CompetitiveEventES>(e => e.Title.Suffix(CompetitiveEventES.TextSuffix)),
                    Infer.Field<CompetitiveEventES>(e => e.ShortTitle),
                    Infer.Field<CompetitiveEventES>(e => e.CompetitiveEventDescriptionItems),
                    Infer.Field<CompetitiveEventES>(e => e.AdditionalDescription),
                    Infer.Field<CompetitiveEventES>(e => e.CompetitiveEventAccountingType),
                    Infer.Field<CompetitiveEventES>(e => e.DescriptionOfTheEnrollmentProcedure),
                    Infer.Field<CompetitiveEventES>(e => e.VenueName),
                    Infer.Field<CompetitiveEventES>(e => e.TermsOfParticipation),
                    Infer.Field<CompetitiveEventES>(e => e.PreferentialTermsOfParticipation),
                    Infer.Field<CompetitiveEventES>(e => e.Benefits),
                    Infer.Field<CompetitiveEventES>(e => e.DescriptionOfOptionsForPeopleWithDisabilities),
                    Infer.Field<CompetitiveEventES>(e => e.Coverage),
                },

                // Query allows results where up to 2 chars may differ from the search keyword
                Query = $"{filter.SearchText}* OR {filter.SearchText}~",
                AllowLeadingWildcard = false,
            });
        }
    }

    private void AddStatesQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (filter.States.Count != 0)
        {
            query.Filter.Add(new TermsQuery()
            {
                Field = Infer.Field<CompetitiveEventES>(f => f.State),
                Term = new(filter.States
                    .Select(s => FieldValue.String(s.ToString())).ToArray()),
            });
        }
    }

    private void AddPlannedFormatOfClassesQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (filter.PlannedFormatsOfClasses.Count != 0)
        {
            query.Filter.Add(new TermsQuery()
            {
                Field = Infer.Field<CompetitiveEventES>(f => f.PlannedFormatOfClasses),
                Term = new(filter.PlannedFormatsOfClasses
                    .Select(s => FieldValue.String(s.ToString())).ToArray()),
            });
        }
    }

    private void AddOptionsForPeopleWithDisabilitiesQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (filter.OptionsForPeopleWithDisabilities)
        {
            query.Filter.Add(new TermQuery(Infer.Field<CompetitiveEventES>(e => e.OptionsForPeopleWithDisabilities))
            {
                Value = filter.OptionsForPeopleWithDisabilities,
            });
        }
    }

    private void AddAreThereBenefitsQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (filter.AreThereBenefits)
        {
            query.Filter.Add(new TermQuery(Infer.Field<CompetitiveEventES>(e => e.AreThereBenefits))
            {
                Value = filter.AreThereBenefits,
            });
        }
    }

    private void AddCompetitiveSelectionQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (filter.CompetitiveSelection)
        {
            query.Filter.Add(new TermQuery(Infer.Field<CompetitiveEventES>(e => e.CompetitiveSelection))
            {
                Value = filter.CompetitiveSelection,
            });
        }
    }

    private void AddPriceQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (filter.MinPrice > 0 || filter.MaxPrice < int.MaxValue) 
        {
            query.Must.Add(new NumberRangeQuery(Infer.Field<CompetitiveEventES>(w => w.Price))
            {
                Gte = filter.MinPrice,
                Lte = filter.MaxPrice
            });
        }    
    }

    private void AddAgeQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {               
        if (filter.MinimumAge != 0)
        {
            query.Must.Add(new NumberRangeQuery(Infer.Field<CompetitiveEventES>(w => w.MinimumAge))
            {
                Gte = filter.MinimumAge,                
            });
        }

        if (filter.MaximumAge != 100)
        {
            query.Must.Add(new NumberRangeQuery(Infer.Field<CompetitiveEventES>(w => w.MinimumAge))
            {
                Lte = filter.MaximumAge,
            });
        }
    }

    private void AddRegistrationEndTimeQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (filter.MinRegistrationEndTime != DateTimeOffset.MinValue || 
            filter.MaxRegistrationEndTime != DateTimeOffset.MaxValue)
        {
            query.Must.Add(new DateRangeQuery(Infer.Field<CompetitiveEventES>(w => w.RegistrationEndTime))
            {
                Gte = filter.MinRegistrationEndTime.ToString(),
                Lte = filter.MaxRegistrationEndTime.ToString(),
            });
        }        
    }

    private void AddScheduledStartTimeQuery(BoolQuery query, CompetitiveEventFilterES filter)
    {
        if (filter.MinScheduledStartTime != DateTimeOffset.MinValue ||
            filter.MaxScheduledStartTime != DateTimeOffset.MaxValue)
        {
            query.Must.Add(new DateRangeQuery(Infer.Field<CompetitiveEventES>(w => w.ScheduledStartTime))
            {
                Gte = filter.MinScheduledStartTime.ToString(),
                Lte = filter.MaxScheduledStartTime.ToString(),
            });
        }
    }

    public override Task<PriceRangeES> GetPriceRangeAsync(CompetitiveEventFilterES filter = null)
    {
        throw new NotImplementedException();
    }
}
