using System.Collections.Generic;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class SearchResultES<TEntity>
    {
        public int TotalAmount { get; set; }

        public IReadOnlyCollection<TEntity> Entities { get; set; }
    }
}
