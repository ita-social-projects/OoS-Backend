using System;
using System.Linq.Expressions;

namespace OutOfSchool.WebApi.Common.SearchFilters
{
    public class EntitySearchFilter<TEntity>
    {
        public EntitySearchFilter(
            Expression<Func<TEntity, bool>> predicate,
            string includedProperties)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            IncludedProperties = includedProperties ?? string.Empty;
        }

        public Expression<Func<TEntity, bool>> Predicate { get; }

        public string IncludedProperties { get; }
    }
}
