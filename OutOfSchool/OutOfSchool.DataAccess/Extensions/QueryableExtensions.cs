using System;
using System.Linq;

namespace OutOfSchool.Services.Extensions;

public static class QueryableExtensions
{
    public static TSource If<TSource>(this TSource source, bool condition, Func<TSource, TSource> filter)
        where TSource : IQueryable
        => condition ? filter(source) : source;
}