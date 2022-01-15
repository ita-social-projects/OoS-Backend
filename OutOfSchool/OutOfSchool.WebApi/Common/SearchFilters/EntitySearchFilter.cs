using System;
using System.Linq.Expressions;

namespace OutOfSchool.WebApi.Common.SearchFilters
{
    /// <summary>
    /// Represents a filter class to find entities from repositories.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
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
