using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.ElasticsearchData.Enums;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.ElasticsearchData;

/// <inheritdoc/>
public class ESWorkshopProvider : ElasticsearchProvider<WorkshopES, WorkshopFilterES>,
    IElasticsearchProvider<WorkshopES, WorkshopFilterES>
{
    public ESWorkshopProvider(ElasticClient elasticClient)
        : base(elasticClient)
    {
    }

    /// <inheritdoc/>
    public override async Task<SearchResultES<WorkshopES>> Search(WorkshopFilterES filter = null)
    {
        if (filter is null)
        {
            filter = new WorkshopFilterES();
        }

        var query = this.CreateQueryFromFilter(filter);
        var sorts = this.CreateSortFromFilter(filter);
        var req = new SearchRequest<WorkshopES>()
        {
            Query = query,
            Sort = sorts,
            From = filter.From,
            Size = filter.Size,
        };

        var resp = await ElasticClient.SearchAsync<WorkshopES>(req);

        return new SearchResultES<WorkshopES>() { TotalAmount = (int)resp.Total, Entities = resp.Documents };
    }

    private QueryContainer CreateQueryFromFilter(WorkshopFilterES filter)
    {
        var queryContainer = new QueryContainer();

        if (filter.Ids.Any())
        {
            queryContainer &= new TermsQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.Id),
                Terms = filter.Ids.Cast<object>(),
            };

            return queryContainer;
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            queryContainer &= new MultiMatchQuery()
            {
                Fields = Infer.Field<WorkshopES>(w => w.Title)
                    .And(Infer.Field<WorkshopES>(w => w.ProviderTitle))
                    .And(Infer.Field<WorkshopES>(w => w.Keywords))
                    .And(Infer.Field<WorkshopES>(w => w.Description)),
                Query = filter.SearchText,
                Fuzziness = Fuzziness.Auto,
            };
        }

        if (filter.DirectionIds.Any())
        {
            queryContainer &= new TermsQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.DirectionIds),
                Terms = filter.DirectionIds.Cast<object>(),
            };
        }

        if (filter.IsFree && (filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            queryContainer &= new TermQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.Price),
                Value = 0,
            };
        }
        else if (!filter.IsFree && !(filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            queryContainer &= new NumericRangeQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.Price),
                GreaterThanOrEqualTo = filter.MinPrice,
                LessThanOrEqualTo = filter.MaxPrice,
            };
        }
        else if (filter.IsFree && !(filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            var tempQuery = new QueryContainer();

            tempQuery = new NumericRangeQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.Price),
                GreaterThanOrEqualTo = filter.MinPrice,
                LessThanOrEqualTo = filter.MaxPrice,
            };

            tempQuery |= new TermQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.Price),
                Value = 0,
            };

            queryContainer &= tempQuery;
        }

        if (filter.MinAge != 0 || filter.MaxAge != 100)
        {
            var ageQuery = new QueryContainer();

            ageQuery = new NumericRangeQuery()
            {
                Field = filter.IsAppropriateAge ? Infer.Field<WorkshopES>(w => w.MinAge) : Infer.Field<WorkshopES>(w => w.MaxAge),
                GreaterThanOrEqualTo = filter.MinAge,
            };

            ageQuery &= new NumericRangeQuery()
            {
                Field = filter.IsAppropriateAge ? Infer.Field<WorkshopES>(w => w.MaxAge) : Infer.Field<WorkshopES>(w => w.MinAge),
                LessThanOrEqualTo = filter.MaxAge,
            };

            queryContainer &= ageQuery;
        }

        if (filter.WithDisabilityOptions)
        {
            queryContainer &= new TermQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.WithDisabilityOptions),
                Value = filter.WithDisabilityOptions,
            };
        }

        if (filter.Statuses.Any())
        {
            queryContainer &= new TermsQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.Status),
                Terms = filter.Statuses.Cast<object>(),
            };
        }

        if (!string.IsNullOrWhiteSpace(filter.Workdays))
        {
            queryContainer &= new NestedQuery()
            {
                Path = Infer.Field<WorkshopES>(p => p.DateTimeRanges),
                Query = new MatchQuery()
                {
                    Field = filter.IsStrictWorkdays
                                ? Infer.Field<WorkshopES>(w => w.DateTimeRanges.First().Workdays.Suffix("keyword"))
                                : Infer.Field<WorkshopES>(w => w.DateTimeRanges.First().Workdays),
                    Query = filter.Workdays,
                },
            };
        }

        if (Equals(OrderBy.Nearest.ToString(), filter.OrderByField))
        {
            queryContainer &= new GeoDistanceQuery()
            {
                Boost = 1.1,
                Name = "named_query",
                Field = Infer.Field<WorkshopES>(w => w.Address.Point),
                DistanceType = GeoDistanceType.Arc,
                Location = new GeoLocation((double)filter.Latitude, (double)filter.Longitude),
                Distance = GeoMathHelper.ElasticRadius,
                ValidationMethod = GeoValidationMethod.IgnoreMalformed,
            };
        }

        if (filter.MinStartTime.TotalMinutes > 0 || filter.MaxStartTime.Hours < 23)
        {
            if (filter.IsAppropriateHours)
            {
                queryContainer &= new NestedQuery()
                {
                    Path = Infer.Field<WorkshopES>(p => p.DateTimeRanges),
                    Query = new NumericRangeQuery()
                    {
                        Field = Infer.Field<WorkshopES>(w => w.DateTimeRanges.First().StartTime),
                        GreaterThanOrEqualTo = filter.MinStartTime.Ticks,
                    },
                };

                queryContainer &= new NestedQuery()
                {
                    Path = Infer.Field<WorkshopES>(p => p.DateTimeRanges),
                    Query = new NumericRangeQuery()
                    {
                        Field = Infer.Field<WorkshopES>(w => w.DateTimeRanges.First().EndTime),
                        LessThan = TimeSpan.FromHours(filter.MaxStartTime.Hours + 1).Ticks,
                    },
                };
            }
            else
            {
                queryContainer &= new NestedQuery()
                {
                    Path = Infer.Field<WorkshopES>(p => p.DateTimeRanges),
                    Query = new NumericRangeQuery()
                    {
                        Field = Infer.Field<WorkshopES>(w => w.DateTimeRanges.First().StartTime),
                        GreaterThanOrEqualTo = filter.MinStartTime.Ticks,
                        LessThan = TimeSpan.FromHours(filter.MaxStartTime.Hours + 1).Ticks,
                    },
                };
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            queryContainer &= new MatchQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.Address.City),
                Query = filter.City,
            };
        }

        if (filter.CATOTTGId > 0)
        {
            var catottgIdFilter = new TermQuery()
            {
                Field = Infer.Field<WorkshopES>(c => c.Address.CATOTTGId),
                Value = filter.CATOTTGId,
            };

            var categoryFilter = new MatchQuery()
            {
                Field = Infer.Field<WorkshopES>(c => c.Address.CodeficatorAddressES.Category),
                Query = CodeficatorCategory.CityDistrict.Name,
            };

            var parentCatottgIdFilter = new TermQuery()
            {
                Field = Infer.Field<WorkshopES>(c => c.Address.CodeficatorAddressES.ParentId),
                Value = filter.CATOTTGId,
            };

            queryContainer &= catottgIdFilter || (categoryFilter && parentCatottgIdFilter);
        }

        return queryContainer;
    }

    private List<ISort> CreateSortFromFilter(WorkshopFilterES filter)
    {
        var sorts = new List<ISort>();

        switch (filter.OrderByField)
        {
            case nameof(OrderBy.Rating):
                sorts.Add(new FieldSort()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Rating),
                    Order = SortOrder.Descending,
                });
                break;

            case nameof(OrderBy.Statistic):
                sorts.Add(new FieldSort()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Rating),
                    Order = SortOrder.Descending,
                });
                break;

            case nameof(OrderBy.PriceAsc):
                sorts.Add(new FieldSort()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Price),
                    Order = SortOrder.Ascending,
                });
                break;

            case nameof(OrderBy.PriceDesc):
                sorts.Add(new FieldSort()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Price),
                    Order = SortOrder.Descending,
                });
                break;

            case nameof(OrderBy.Alphabet):
                break;

            case nameof(OrderBy.Nearest):
                sorts.Add(new GeoDistanceSort()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Address.Point),
                    Points = new[] { new GeoLocation((double)filter.Latitude, (double)filter.Longitude) },
                    Order = SortOrder.Ascending,
                });
                break;

            default:
                sorts.Add(new FieldSort()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Id),
                    Order = SortOrder.Ascending,
                });
                break;
        }

        sorts.Add(new FieldSort
        {
            Field = WorkshopES.TitleKeyword,
            Order = SortOrder.Ascending,
        });

        return sorts;
    }
}
