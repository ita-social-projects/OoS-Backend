using System.Collections.Generic;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class SearchResultES<TEntity>
    {
        public long TotalAmount { get; set; }

        public IReadOnlyCollection<TEntity> Entities { get; set; }
    }
}
