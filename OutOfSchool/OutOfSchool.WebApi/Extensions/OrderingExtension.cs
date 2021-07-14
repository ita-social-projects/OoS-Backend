using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Extensions
{
    public static class OrderingExtension
    {
        public static IQueryable<TEntity> DynamicOrderBy<TEntity>(
            this IQueryable<TEntity> entities,
            Dictionary<Expression<Func<TEntity, object>>, SortDirection> orderBy)
        {
            if (!orderBy.Any())
            {
                return entities;
            }

            var orderedData = orderBy.Values.First() == SortDirection.Ascending
                ? entities.OrderBy(orderBy.Keys.First())
                : entities.OrderByDescending(orderBy.Keys.First());

            foreach (var expression in orderBy.Skip(1))
            {
                orderedData = expression.Value == SortDirection.Ascending
                    ? orderedData.ThenBy(expression.Key)
                    : orderedData.ThenByDescending(expression.Key);
            }

            return orderedData;
        }
    }
}
