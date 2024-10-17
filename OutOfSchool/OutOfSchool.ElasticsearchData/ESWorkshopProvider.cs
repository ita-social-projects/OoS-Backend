using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using OutOfSchool.Common.Enums;
using OutOfSchool.ElasticsearchData.Enums;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.ElasticsearchData;

/// <inheritdoc/>
public class ESWorkshopProvider(ElasticsearchClient elasticClient) :
    ElasticsearchProvider<WorkshopES, WorkshopFilterES>(elasticClient),
    IElasticsearchProvider<WorkshopES, WorkshopFilterES>
{
    /// <inheritdoc/>
    public override async Task<SearchResultES<WorkshopES>> Search(WorkshopFilterES filter = null)
    {
        filter ??= new WorkshopFilterES();

        var query = this.CreateQueryFromFilter(filter);
        var sorts = this.CreateSortFromFilter(filter);
        var request = new SearchRequest<WorkshopES>()
        {
            Query = query,
            Sort = sorts,
            From = filter.From,
            Size = filter.Size,
        };

        var resp = await ElasticClient.SearchAsync<WorkshopES>(request);

        return new SearchResultES<WorkshopES>()
        {
            TotalAmount = (int)resp.Total,
            Entities = resp.Documents,
        };
    }

    private Query CreateQueryFromFilter(WorkshopFilterES filter)
    {
        var query = new BoolQuery
        {
            Filter =
            [
                new TermsQuery()
                {
                    Field = Infer.Field<WorkshopES>(f => f.ProviderStatus),
                    Term = new([$"{ProviderStatus.Approved}", $"{ProviderStatus.Recheck}"]),
                },
                new TermQuery(Infer.Field<WorkshopES>(w => w.IsBlocked))
                {
                    Value = "false",
                },
            ],
            Must = [],
        };

        if (filter.Ids.Count != 0)
        {
            query.Filter.Add(new IdsQuery()
            {
                Values = filter.Ids.Select(id => id.ToString()).ToArray(),
            });
            return query;
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            query.Filter.Add(new TermQuery(Infer.Field<WorkshopES>(w => w.InstitutionId))
            {
                Value = filter.InstitutionId.ToString(),
            });
            return query;
        }

        AddSearchTextQuery(query, filter);
        AddCityQuery(query, filter);
        AddDirectionIdsQuery(query, filter);
        AddPriceQuery(query, filter);
        AddAgeQuery(query, filter);
        AddDisabilityOptionsQuery(query, filter);
        AddStatusesQuery(query, filter);
        AddFormOfLearningQuery(query, filter);
        AddWorkdaysQuery(query, filter);
        AddCATOTTGIdQuery(query, filter);
        AddGeoNearestQuery(query, filter);
        AddTimeQuery(query, filter);

        return query;
    }

    private List<SortOptions> CreateSortFromFilter(WorkshopFilterES filter)
    {
        var sorts = new List<SortOptions>();

        switch (filter.OrderByField)
        {
            case nameof(OrderBy.Rating):
                sorts.Add(SortOptions.Field(
                    Infer.Field<WorkshopES>(w => w.Rating),
                    new FieldSort()
                    {
                        Order = SortOrder.Desc,
                    }));
                break;

            case nameof(OrderBy.Statistic):
                sorts.Add(SortOptions.Field(
                    Infer.Field<WorkshopES>(w => w.Rating),
                    new FieldSort()
                    {
                        Order = SortOrder.Desc,
                    }));
                break;

            case nameof(OrderBy.PriceAsc):
                sorts.Add(SortOptions.Field(
                    Infer.Field<WorkshopES>(w => w.Price),
                    new FieldSort()
                    {
                        Order = SortOrder.Asc,
                    }));
                break;

            case nameof(OrderBy.PriceDesc):
                sorts.Add(SortOptions.Field(
                    Infer.Field<WorkshopES>(w => w.Price),
                    new FieldSort()
                    {
                        Order = SortOrder.Desc,
                    }));
                break;

            case nameof(OrderBy.Alphabet):
                break;

            case nameof(OrderBy.Nearest):
                sorts.Add(new GeoDistanceSort()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Address.Point),
                    Location = [
                    new LatLonGeoLocation()
                    {
                        Lat = (double)filter.Latitude,
                        Lon = (double)filter.Longitude,
                    },],
                    Order = SortOrder.Asc,
                });
                break;

            default:
                sorts.Add(SortOptions.Field(
                    Infer.Field<WorkshopES>(w => w.Id),
                    new FieldSort()
                    {
                        Order = SortOrder.Asc,
                    }));
                break;
        }

        sorts.Add(SortOptions.Field(
            Infer.Field<WorkshopES>(w => w.Title.Suffix(WorkshopES.SortSuffix)),
            new FieldSort
            {
                Order = SortOrder.Asc,
            }));

        return sorts;
    }

    private void AddSearchTextQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            query.Must.Add(new QueryStringQuery()
            {
                Fields = new[]
                {
                    Infer.Field<WorkshopES>(w => w.Title.Suffix(WorkshopES.TextSuffix)),
                    Infer.Field<WorkshopES>(w => w.ShortTitle),
                    Infer.Field<WorkshopES>(w => w.ProviderTitle),
                    Infer.Field<WorkshopES>(w => w.Keywords),
                    Infer.Field<WorkshopES>(w => w.Description),
                },

                // Query allows results where up to 2 chars may differ from the search keyword
                Query = $"{filter.SearchText}* OR {filter.SearchText}~",
                AllowLeadingWildcard = false,
            });
        }
    }

    private void AddCityQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query.Must.Add(new MatchQuery(Infer.Field<WorkshopES>(w => w.Address.City))
            {
                Query = filter.City,
            });
        }
    }

    private void AddDirectionIdsQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (filter.DirectionIds.Count != 0)
        {
            query.Filter.Add(new TermsQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.DirectionIds),
                Term = new(filter.DirectionIds
                    .Select(id => FieldValue.String(id.ToString())).ToArray()),
            });
        }
    }

    private void AddPriceQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (filter.IsFree && (filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            query.Must.Add(new TermQuery(Infer.Field<WorkshopES>(w => w.Price))
            {
                Value = 0,
            });
        }
        else if (!filter.IsFree && !(filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            query.Must.Add(new NumberRangeQuery(Infer.Field<WorkshopES>(w => w.Price))
            {
                Gte = filter.MinPrice,
                Lte = filter.MaxPrice,
            });
        }
        else if (filter.IsFree && !(filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            query.Must.Add(new BoolQuery()
            {
                Should =
                [
                    new NumberRangeQuery(Infer.Field<WorkshopES>(w => w.Price))
                    {
                        Gte = filter.MinPrice,
                        Lte = filter.MaxPrice,
                    },
                    new TermQuery(Infer.Field<WorkshopES>(w => w.Price))
                    {
                        Value = 0,
                    },
                ],
            });
        }
    }

    private void AddAgeQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (filter.MinAge != 0 || filter.MaxAge != 100)
        {
            query.Must.Add(
                new NumberRangeQuery(filter.IsAppropriateAge ?
                    Infer.Field<WorkshopES>(w => w.MinAge) :
                    Infer.Field<WorkshopES>(w => w.MaxAge))
                {
                    Gte = filter.MinAge,
                });
            query.Must.Add(
                new NumberRangeQuery(filter.IsAppropriateAge ?
                        Infer.Field<WorkshopES>(w => w.MaxAge) :
                        Infer.Field<WorkshopES>(w => w.MinAge))
                {
                    Lte = filter.MaxAge,
                });
        }
    }

    private void AddDisabilityOptionsQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (filter.WithDisabilityOptions)
        {
            query.Filter.Add(new TermQuery(Infer.Field<WorkshopES>(w => w.WithDisabilityOptions))
            {
                Value = filter.WithDisabilityOptions,
            });
        }
    }

    private void AddStatusesQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (filter.Statuses.Count != 0)
        {
            query.Filter.Add(new TermsQuery()
            {
                Field = Infer.Field<WorkshopES>(f => f.Status),
                Term = new(filter.Statuses
                    .Select(s => FieldValue.String(s.ToString())).ToArray()),
            });
        }
    }

    private void AddFormOfLearningQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (filter.FormOfLearning.Count != 0)
        {
            query.Filter.Add(new TermsQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.FormOfLearning),
                Term = new TermsQueryField(filter.FormOfLearning
                    .Select(f => FieldValue.String(f.ToString())).ToArray()),
            });
        }
    }

    private void AddWorkdaysQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Workdays))
        {
            query.Must.Add(new NestedQuery()
            {
                Path = Infer.Field<WorkshopES>(p => p.DateTimeRanges),
                Query = new MatchQuery(filter.IsStrictWorkdays
                    ? Infer.Field<WorkshopES>(w =>
                        w.DateTimeRanges[0].Workdays.Suffix(WorkshopES.KeywordSuffix))
                    : Infer.Field<WorkshopES>(w => w.DateTimeRanges[0].Workdays))
                {
                    Query = filter.Workdays,
                },
            });
        }
    }

    private void AddCATOTTGIdQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (filter.CATOTTGId > 0)
        {
            query.Must.Add(new BoolQuery()
            {
                Should =
                [
                    new TermQuery(Infer.Field<WorkshopES>(c => c.Address.CATOTTGId))
                    {
                        Value = filter.CATOTTGId,
                    },
                    new BoolQuery()
                    {
                        Must =
                        [
                            new MatchQuery(Infer.Field<WorkshopES>(c =>
                                c.Address.CodeficatorAddressES.Category))
                            {
                                Query = CodeficatorCategory.CityDistrict.Name,
                            },
                            new TermQuery(Infer.Field<WorkshopES>(c =>
                                c.Address.CodeficatorAddressES.ParentId))
                            {
                                Value = filter.CATOTTGId,
                            },
                        ],
                    },
                ],
                MinimumShouldMatch = 1,
            });
        }
    }

    private void AddGeoNearestQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (Equals(OrderBy.Nearest.ToString(), filter.OrderByField))
        {
            query.Must.Add(new GeoDistanceQuery()
            {
                Boost = 1.1F,
                QueryName = "named_query",
                Field = "address.point",
                DistanceType = GeoDistanceType.Arc,
                Location = new LatLonGeoLocation()
                {
                    Lat = (double)filter.Latitude,
                    Lon = (double)filter.Longitude,
                },
                Distance = filter.ElasticRadius,
                ValidationMethod = GeoValidationMethod.IgnoreMalformed,
            });
        }
    }

    private void AddTimeQuery(BoolQuery query, WorkshopFilterES filter)
    {
        if (filter.MinStartTime.TotalMinutes > 0 || filter.MaxStartTime < WorkshopFilterES.MaxTimeInDay)
        {
            if (filter.IsAppropriateHours)
            {
                query.Must.Add(new NestedQuery()
                {
                    Path = Infer.Field<WorkshopES>(p => p.DateTimeRanges),
                    Query = new BoolQuery()
                    {
                        Must =
                        [
                            new DateRangeQuery(
                                Infer.Field<WorkshopES>(w => w.DateTimeRanges[0].StartTime))
                            {
                                Gte = filter.MinStartTime.ToString(),
                            },
                            new DateRangeQuery(
                                Infer.Field<WorkshopES>(w => w.DateTimeRanges[0].EndTime))
                            {
                                Lte = filter.MaxStartTime.ToString(),
                            },
                        ],
                    },
                });
            }
            else
            {
                query.Must.Add(new NestedQuery()
                {
                    Path = Infer.Field<WorkshopES>(p => p.DateTimeRanges),
                    Query = new DateRangeQuery(
                        Infer.Field<WorkshopES>(w => w.DateTimeRanges[0].StartTime))
                    {
                        Gte = filter.MinStartTime.ToString(),
                        Lte = filter.MaxStartTime.ToString(),
                    },
                });
            }
        }
    }
}