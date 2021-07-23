using System.Collections.Generic;

namespace OutOfSchool.WebApi.Models
{
    public class SearchResult<TEntity>
    {
        public long TotalAmount { get; set; }

        public IReadOnlyCollection<TEntity> Entities { get; set; }
    }
}
