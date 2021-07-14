using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public override async Task<IEnumerable<WorkshopES>> Search(WorkshopFilterES filter = null)
        {
            if (filter is null)
            {
                return await base.Search(filter);
            }

            var query = this.CreateQueryFromFilter(filter);
            var sorts = this.CreateSortFromFilter(filter);

            var resp = await ElasticClient.SearchAsync<WorkshopES>(new SearchRequest<WorkshopES>()
                {
                    Query = query,
                    Sort = sorts,
                });

            return resp.Documents;
        }

        private QueryContainer CreateQueryFromFilter(WorkshopFilterES filter)
        {
            QueryContainer queryContainer = new QueryContainer();

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                queryContainer &= new MultiMatchQuery()
                {
                    Fields = Infer.Field<WorkshopES>(w => w.Title).And(Infer.Field<WorkshopES>(w => w.ProviderTitle)).And(Infer.Field<WorkshopES>(w => w.Keywords)),
                    Query = filter.SearchText,
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

            if (filter.MinPrice >= 0 && filter.MaxPrice < int.MaxValue)
            {
                queryContainer &= new NumericRangeQuery()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Price),
                    GreaterThanOrEqualTo = filter.MinPrice,
                    LessThanOrEqualTo = filter.MaxPrice,
                };
            }

            if (filter.MinPrice == 0 && filter.MaxPrice == 0)
            {
                queryContainer &= new TermQuery()
                {
                    Field = Infer.Field<WorkshopES>(w => w.Price),
                    Value = filter.MaxPrice,
                };
            }

            if (filter.Ages[0].MinAge != 0 || filter.Ages[0].MaxAge != 100)
            {
                var ageQuery = new QueryContainer();

                foreach (var age in filter.Ages)
                {
                    var ageQueryItem = new QueryContainer();

                    ageQueryItem = new NumericRangeQuery()
                    {
                        Field = Infer.Field<WorkshopES>(w => w.MinAge),
                        LessThanOrEqualTo = age.MaxAge,
                    };

                    ageQueryItem &= new NumericRangeQuery()
                    {
                        Field = Infer.Field<WorkshopES>(w => w.MaxAge),
                        GreaterThanOrEqualTo = age.MinAge,
                    };

                    ageQuery |= ageQueryItem;
                }

                queryContainer &= ageQuery;
            }

            queryContainer &= new MatchQuery()
            {
                Field = Infer.Field<WorkshopES>(w => w.Address.City),
                Query = filter.City,
            };

            return queryContainer;
        }

        private List<ISort> CreateSortFromFilter(WorkshopFilterES filter)
        {
            var sorts = new List<ISort>();

            switch (filter.OrderByField)
            {
                case OrderBy.Rating:
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Rating), Order = SortOrder.Descending });
                    break;

                case OrderBy.Statistic:
                    break;

                case OrderBy.Price:
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Price), Order = SortOrder.Ascending });
                    break;

                case OrderBy.Alphabet:
                    sorts.Add(new FieldSort() { Field = "title.keyword", Order = SortOrder.Ascending });
                    break;

                case OrderBy.Nearest:
                    break;

                default:
                    sorts.Add(new FieldSort() { Field = Infer.Field<WorkshopES>(w => w.Rating), Order = SortOrder.Descending });
                    break;
            }

            return sorts;
        }
    }
}
