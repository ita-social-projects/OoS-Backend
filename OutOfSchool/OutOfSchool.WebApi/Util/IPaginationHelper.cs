using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Util;

/// <summary>
/// Interface for pagination helper.
/// </summary>
/// <typeparam name="T">Entity wich list we want to paginate for.</typeparam>
public interface IPaginationHelper<T>
{
    /// <summary>
    /// Get general amount of pages with definite watchsize with specific filter or without it.
    /// </summary>
    /// <param name="pageSize">The amount of records on the one page.</param>
    /// <param name="where">The filter that we want to apply to the recorrds list.</param>
    /// <returns>The amount of pages.</returns>
    Task<int> GetCountOfPages(int pageSize, Expression<Func<T, bool>> where = null);

    /// <summary>
    /// Get the ordered, filtered records for the page.
    /// </summary>
    /// <typeparam name="TOrderKey">The type that we want to order list with.</typeparam>
    /// <param name="pageNumber">The number of the page that we want to get.</param>
    /// <param name="pageSize">The amount of records on the one page.</param>
    /// <param name="includeProperties">What Properties we want to include to objects that we will receive.</param>
    /// <param name="where">Filter.</param>
    /// <param name="orderBy">Filter that defines by wich property we want to order by.</param>
    /// <param name="ascending">Ascending or descending ordering.</param>
    /// <returns>Ordered, filtered list of elements.</returns>
    Task<List<T>> GetPage<TOrderKey>(
        int pageNumber, int pageSize, string includeProperties = "", Expression<Func<T, bool>> where = null, Expression<Func<T, object>> orderBy = null, bool ascending = true);
}