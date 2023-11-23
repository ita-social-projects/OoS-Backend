using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.Services.Extensions;

public static class QueryableExtensions
{
    public const char PROPERTIES_SEPARATOR = ',';
    private static char[] propertiesSplitSymbols = new char[] { PROPERTIES_SEPARATOR };

    public static IQueryable<T> IncludeProperties<T>(this IQueryable<T> query, string properties)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(properties))
        {
            return query;
        }

        foreach (var property in properties.Split(propertiesSplitSymbols, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(property);
        }

        return query;
    }

    public static TSource If<TSource>(this TSource source, bool condition, Func<TSource, TSource> filter)
        where TSource : IQueryable
        => condition ? filter(source) : source;
}