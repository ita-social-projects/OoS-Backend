using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace OutOfSchool.Common.Extensions;

public static class PaginationExtensions
{
    /// <summary>
    /// Extension method for <see cref="IQueryable{T}"/> to convert it to a paginated result <see cref="PaginatedResult{T}"/> asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source query.</typeparam>
    /// <param name="query">The source query to paginate.</param>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="PaginatedResult{T}"/> 
    /// that contains the paginated items.
    /// </returns>
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        totalPages = Math.Max(totalPages, 1);

        // Ensure page is within valid range
        page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages,
        };
    }
}


