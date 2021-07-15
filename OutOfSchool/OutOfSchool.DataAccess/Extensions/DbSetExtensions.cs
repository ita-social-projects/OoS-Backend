using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

#nullable enable

namespace OutOfSchool.Services.Extensions
{
    public static class DbSetExtensions
    {
        /// <summary>
        /// Add entry if it doesn't exist based on a predicate.
        /// Not thread safe! There's a window between Any and Add where another thread can add an entry.
        /// Performance issues is need to add multiple entities.
        /// </summary>
        /// <param name="dbSet">Extension target.</param>
        /// <param name="entity">Entry to add.</param>
        /// <param name="predicate">Optional predicate.</param>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <returns>Return an entity that was just added or null.</returns>
        public static EntityEntry<T>? AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>>? predicate = null)
            where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }
    }
}