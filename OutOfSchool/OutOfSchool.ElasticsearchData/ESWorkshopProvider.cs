using System.Collections.Generic;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using OutOfSchool.ElasticsearchData.Enums;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.ElasticsearchData
{
    /// <inheritdoc/>
    public class ESWorkshopProvider : ElasticsearchProvider<WorkshopES, WorkshopFilterES>, IElasticsearchProvider<WorkshopES, WorkshopFilterES>
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

            var resp = await ElasticClient.SearchAsync<WorkshopES>(new SearchRequest<WorkshopES>()
                {
                    Query = query,
                    Sort = sorts,
                    From = filter.From,
                    Size = filter.Size,
                });

            return new SearchResultES<WorkshopES>() { TotalAmount = (int)resp.Total, Entities = resp.Documents };
        }

        private QueryContainer CreateQueryFromFilter(WorkshopFilterES filter)
        {
            var queryContainer = new QueryContainer();

            if (!(filter.Ids is null) && filter.Ids.Count > 0)
            {
                var box = new List<object>();
                foreach (var item in filter.Ids)
                {
                    box.Add(item);
                }

                queryContainer &= new TermsQuery()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Id),
                    Terms = box,
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

            if (filter.DirectionIds[0] != 0)
            {
                var box = new List<object>();
                foreach (var item in filter.DirectionIds)
                {
                    box.Add(item);
                }

                queryContainer &= new TermsQuery()
                {
                    Field = Infer.Field<WorkshopES>(w => w.DirectionId),
                    Terms = box,
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
                    Field = Infer.Field<WorkshopES>(w => w.MinAge),
                    LessThanOrEqualTo = filter.MaxAge,
                };

                ageQuery &= new NumericRangeQuery()
                {
                    Field = Infer.Field<WorkshopES>(w => w.MaxAge),
                    GreaterThanOrEqualTo = filter.MinAge,
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

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                queryContainer &= new MatchQuery()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Address.City),
                    Query = filter.City,
                };
            }

            return queryContainer;
        }

        private List<ISort> CreateSortFromFilter(WorkshopFilterES filter)
        {
            var sorts = new List<ISort>();

            switch (filter.OrderByField)
            {
                case nameof(OrderBy.Rating):
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Rating), Order = SortOrder.Descending });
                    break;

                case nameof(OrderBy.Statistic):
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Rating), Order = SortOrder.Descending });
                    break;

                case nameof(OrderBy.PriceAsc):
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Price), Order = SortOrder.Ascending });
                    break;

                case nameof(OrderBy.PriceDesc):
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Price), Order = SortOrder.Descending });
                    break;

                case nameof(OrderBy.Alphabet):
                    sorts.Add(new FieldSort() { Field = "title.keyword", Order = SortOrder.Ascending });
                    break;

                case nameof(OrderBy.Nearest):
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Rating), Order = SortOrder.Descending });
                    break;

                default:
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Id), Order = SortOrder.Ascending });
                    break;
            }

            return sorts;
        }
    }
}
