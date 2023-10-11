using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IEntityRepositoryBase<TKey, TEntity> : IEntityAddOnlyRepository<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Read element from database by id and update information.
    /// </summary>
    /// <param name="dto">New data with information.</param>
    /// <param name="map">Method that represents mapping operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was updated.</returns>
    /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    Task<TEntity> ReadAndUpdateWith<TDto>(TDto dto, Func<TDto, TEntity, TEntity> map)
        where TDto : IDto<TEntity, TKey>;

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

public interface IEntityRepositorySoftDeleted<TKey, TEntity> : IEntityRepositoryBase<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, ISoftDeleted, new()
    where TKey : IEquatable<TKey>
{
}

public interface ISensitiveEntityRepositorySoftDeleted<TEntity> : IEntityRepositorySoftDeleted<Guid, TEntity>
    where TEntity : class, IKeyedEntity<Guid>, ISoftDeleted, new()
{
}