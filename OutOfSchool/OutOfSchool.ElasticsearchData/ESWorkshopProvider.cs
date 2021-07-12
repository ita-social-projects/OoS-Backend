using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
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
        public override async Task<IEnumerable<WorkshopES>> Search(WorkshopFilterES filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException($"The argument {filter} was not set to instance.");
            }

            ISearchResponse<WorkshopES> resp = await ElasticClient.SearchAsync<WorkshopES>(
                s => s.Query(
                    q => q.MatchAll()));

            return resp.Documents;
        }
    }
}
