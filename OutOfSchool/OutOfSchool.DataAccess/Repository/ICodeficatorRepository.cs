using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface ICodeficatorRepository : IEntityRepositorySoftDeleted<long, CATOTTG>
{
    /// <summary>
    /// Get elements pair values (Id, Name) by a specific filter.
    /// </summary>
    /// <param name="predicate">Filter with key.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{KeyValuePair}"/> that contains elements' Id and Name.</returns>
    public Task<IEnumerable<KeyValuePair<long, string>>> GetNamesByFilter(Expression<Func<CATOTTG, bool>> predicate);

    /// <summary>
    /// Get elements' list by a part of name.
    /// </summary>
    /// <param name="namePart">Part of name for search.</param>
    /// <param name="categories">Categories for search.</param>
    /// <param name="parentId">Id for parent codeficator.</param>
    /// <returns>The task result contains a <see cref="List{CodeficatorAddressDto}"/> that contains elements' full addresses.</returns>
    public Task<List<CodeficatorAddressDto>> GetFullAddressesByPartOfName(string namePart, string categories = default, long parentId = 0);

    /// <summary>
    /// Get the list of CATOTTGs Ids by list of CATOTTGs parentIds.
    /// </summary>
    /// <param name="parentIds">list of CATOTTGs parentIds</param>
    /// <returns>The task result contains a <see cref="List{TResult}"/> that contains the list of CATOTTGs Ids.</returns>
    public Task<List<long>> GetIdsByParentIds(List<long> parentIds);
}