using System.Collections.Generic;

namespace OutOfSchool.WebApi.Models
{
    public class SearchResult<TEntity>
    {
        public int TotalAmount { get; set; }

        public IReadOnlyCollection<TEntity> Entities { get; set; }
    }
}
