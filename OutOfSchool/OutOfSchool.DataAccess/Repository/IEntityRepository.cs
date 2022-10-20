using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IEntityRepositoryBase<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
    IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Add new element.
    /// </summary>
    /// <param name="entity">Entity to create.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was created.</returns>
    /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    Task<TEntity> Create(TEntity entity);

    /// <summary>
    /// Add new elements.
    /// </summary>
    /// <param name="entities">Entities to create.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entities that were created.</returns>
    /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> entities);

    /// <summary>
    /// Runs operation in transaction.
    /// </summary>
    /// <param name="operation">Method that represents the operation.</param>
    /// <typeparam name="T">The type operation must return after transaction.</typeparam>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<T> RunInTransaction<T>(Func<Task<T>> operation);

    /// <summary>
    /// Update information about element.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was updated.</returns>
    /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    Task<TEntity> Update(TEntity entity);

    /// <summary>
    /// Delete element.
    /// </summary>
    /// <param name="entity">Entity to delete.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    Task Delete(TEntity entity);

    /// <summary>
    /// Get all elements.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{T}"/> that contains elements.</returns>
    Task<IEnumerable<TEntity>> GetAll();

    /// <summary>
    /// Get element by Id.
    /// </summary>
    /// <param name="id">Key in database.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains an entity that was found, or null.</returns>
    Task<TEntity> GetById(TKey id);

    /// <summary>
    /// Get element by Id.
    /// </summary>
    /// <param name="id">Key in database.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains an entity that was found, or null.</returns>
    //Task<TEntity> GetByEmail(string Email);

    /// <summary>
    /// Get all elements with details.
    /// </summary>
    /// <param name="includeProperties">Name of properties which should be included.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{T}"/> that contains elements.</returns>
    Task<IEnumerable<TEntity>> GetAllWithDetails(string includeProperties = "");

    /// <summary>
    /// Get elements by a specific filter.
    /// </summary>
    /// <param name="predicate">Filter with key.</param>
    /// <param name="includeProperties">Name of properties which should be included.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{T}"/> that contains elements.</returns>
    Task<IEnumerable<TEntity>> GetByFilter(Expression<Func<TEntity, bool>> predicate, string includeProperties = "");

    /// <summary>
    /// Get elements by a specific filter with no tracking.
    /// </summary>
    /// <param name="predicate">Filter with key.</param>
    /// <param name="includeProperties">Name of properties which should be included.</param>
    /// <returns>An <see cref="IQueryable{TResult}"/> that contains elements from the input sequence that
    /// satisfy the condition specified by predicate.
    IQueryable<TEntity> GetByFilterNoTracking(Expression<Func<TEntity, bool>> predicate, string includeProperties = "");

    /// <summary>
    /// Get the amount of elements with filter or without it.
    /// </summary>
    /// <param name="where">Filter.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains an amount of found elements.</returns>
    Task<int> Count(Expression<Func<TEntity, bool>> where = null);

    /// <summary>
    /// Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    /// <param name="where">Filter.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains <see langword="true" /> if any elements in the source sequence pass the test in the specified
    /// filter; otherwise, <see langword="false" />.</returns>
    Task<bool> Any(Expression<Func<TEntity, bool>> where = null);

    /// <summary>
    /// Get ordered, filtered list of elements.
    /// </summary>
    /// <param name="skip">How many records we want tp skip.</param>
    /// <param name="take">How many records we want to take.</param>
    /// <param name="includeProperties">What Properties we want to include to objects that we will receive.</param>
    /// <param name="where">Filter.</param>
    /// <param name="orderBy">Filter that defines by wich properties we want to order by with ascending or descending ordering.</param>
    /// <param name="asNoTracking">Define if the result set will be tracked by the context.</param>
    /// <returns>An <see cref="IQueryable{TResult}"/> that contains elements from the input sequence that
    /// satisfy the condition specified by predicate. An ordered, filtered <see cref="IQueryable{T}"/>.</returns>
    IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        string includeProperties = "",
        Expression<Func<TEntity, bool>> where = null,
        Dictionary<Expression<Func<TEntity, object>>, SortDirection> orderBy = null,
        bool asNoTracking = false);
}

/// <summary>
/// Interface of repository for accessing the database.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TEntity">Entity type.</typeparam>
public interface IEntityRepository<TKey, TEntity> : IEntityRepositoryBase<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
}

public interface ISensitiveEntityRepository<TEntity> : IEntityRepositoryBase<Guid, TEntity>
    where TEntity : class, IKeyedEntity<Guid>, new()
{
}