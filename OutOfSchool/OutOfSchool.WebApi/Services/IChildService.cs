using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for Child entity.
/// </summary>
public interface IChildService
{
    /// <summary>
    /// Create a new child for specified user.
    /// If child's property ParentId is not equal to the parent's Id that was found by specified userId,
    /// the child's property will be changed to the proper value: parent's Id that was found.
    /// </summary>
    /// <param name="childDto">Child to add.</param>
    /// <param name="userId">Key in the User table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The result contains a <see cref="ChildDto"/> that was created.</returns>
    /// <exception cref="ArgumentNullException">If one of the parameters was null.</exception>
    /// <exception cref="ArgumentException">If required child's properties are not set.</exception>
    /// <exception cref="UnauthorizedAccessException">If parent with userId was not found.</exception>
    /// <exception cref="DbUpdateException">If something wrong occurred while saving to the database.</exception>
    Task<ChildDto> CreateChildForUser(ChildDto childDto, string userId);

    /// <summary>
    /// Get all children from the database.
    /// </summary>
    /// <param name="offsetFilter">Filter to get a part of all children that were found.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
    /// <exception cref="ArgumentNullException">If one of the parameters was null.</exception>
    /// <exception cref="ArgumentException">If one of the offsetFilter's properties is negative.</exception>
    /// <exception cref="SqlException">If the database cannot execute the query.</exception>
    Task<SearchResult<ChildDto>> GetAllWithOffsetFilterOrderedById(OffsetFilter offsetFilter);

    /// <summary>
    /// Get a child by it's key and userId.
    /// </summary>
    /// <param name="id">Key in the Children table.</param>
    /// <param name="userId">Key in the Users table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The result contains a <see cref="ChildDto"/> that was found.
    /// If the child was not found or the user is trying to get not his own child the <see cref="UnauthorizedAccessException"/> will be thrown.</returns>
    /// <exception cref="ArgumentException">If one of the parameters was not valid.</exception>
    /// <exception cref="UnauthorizedAccessException">If the child was not found or the user is trying to get not his own child.</exception>
    /// <exception cref="SqlException">If the database cannot execute the query.</exception>
    Task<ChildDto> GetByIdAndUserId(Guid id, string userId);

    /// <summary>
    /// Get children with some ParentId.
    /// </summary>
    /// <param name="parentId">ParentId.</param>
    /// <param name="offsetFilter">Filter to get a part of all children that were found.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
    /// <exception cref="ArgumentNullException">If one of the parameters was null.</exception>
    /// <exception cref="ArgumentException">If one of the parameters was not valid.</exception>
    /// <exception cref="SqlException">If the database cannot execute the query.</exception>
    Task<SearchResult<ChildDto>> GetByParentIdOrderedByFirstName(Guid parentId, OffsetFilter offsetFilter);

    /// <summary>
    /// Get children with some UserId.
    /// </summary>
    /// <param name="userId">Key in the User table.</param>
    /// <param name="offsetFilter">Filter to get a part of all children that were found.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
    /// <exception cref="ArgumentNullException">If one of the parameters was null.</exception>
    /// <exception cref="ArgumentException">If one of the parameters was not valid.</exception>
    /// <exception cref="SqlException">If the database cannot execute the query.</exception>
    Task<SearchResult<ChildDto>> GetByUserId(string userId, OffsetFilter offsetFilter);

    /// <summary>
    /// Update a child of the specified user.
    /// Child's property ParentId cannot be changed and uatomatically will be set to the old value.
    /// </summary>
    /// <param name="childDto">Child entity to update.</param>
    /// <param name="userId">Key in the User table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The result contains a <see cref="ChildDto"/> that was updated.</returns>
    /// <exception cref="ArgumentNullException">If one of the entities was not initialized.</exception>
    /// <exception cref="ArgumentException">If required child's properties are not set.</exception>
    /// <exception cref="UnauthorizedAccessException">If user is trying to update not his own child.</exception>
    /// <exception cref="DbUpdateException">If something wrong occurred while saving to the database.</exception>
    Task<ChildDto> UpdateChildCheckingItsUserIdProperty(ChildDto childDto, string userId);

    /// <summary>
    /// Delete a child of the specified user.
    /// </summary>
    /// <param name="id">Child's key.</param>
    /// <param name="userId">Key in the User table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">If required child's properties are not set.</exception>
    /// <exception cref="UnauthorizedAccessException">If user is trying to delete not his own child.</exception>
    /// <exception cref="DbUpdateException">If something wrong occurred while saving to the database.</exception>
    Task DeleteChildCheckingItsUserIdProperty(Guid id, string userId);
}