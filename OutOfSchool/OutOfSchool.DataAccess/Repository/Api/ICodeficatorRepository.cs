using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

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
    
    /// <summary>
    /// Get the Code property of a CATOTTG entity by its Id.
    /// </summary>
    /// <param name="id">The Id of the CATOTTG entity.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.  
    /// The task result contains the Code as a <see cref="string"/> if the entity exists; otherwise, <c>null</c>.
    /// </returns>
    public Task<string?> GetCodeByIdAsync(long id);

    /// <summary>
    /// Gets the Id of a CATOTTG entity by its Code.
    /// </summary>
    /// <param name="code">The Code of the CATOTTG entity.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
    /// The task result contains the Id as a <see cref="long"/> if an entity with the specified code exists; otherwise, <c>null</c>.
    /// </returns>
    Task<long?> GetIdByCodeAsync(string code);

}