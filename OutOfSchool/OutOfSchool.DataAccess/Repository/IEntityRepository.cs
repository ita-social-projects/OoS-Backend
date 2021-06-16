﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.Services.Repository
{
    /// <summary>
    /// Interface of repository for accessing the database.
    /// </summary>
    /// <typeparam name="T">Entity.</typeparam>
    public interface IEntityRepository<T>
        where T : class, new()
    {
        /// <summary>
        /// Add new element.
        /// </summary>
        /// <param name="entity">Entity to create.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the entity that was created.</returns>
        /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task<T> Create(T entity);

        /// <summary>
        /// Runs operation in transaction.
        /// </summary>
        /// <param name="operation">Method that represents the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> RunInTransaction(Func<Task<T>> operation);

        /// <summary>
        /// Update information about element.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the entity that was updated.</returns>
        /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task<T> Update(T entity);

        /// <summary>
        /// Delete element.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task Delete(T entity);

        /// <summary>
        /// Get all elements.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{T}"/> that contains elements.</returns>
        Task<IEnumerable<T>> GetAll();

        /// <summary>
        /// Get element by Id.
        /// </summary>
        /// <param name="id">Key in database.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation. 
        /// The task result contains an entity that was found, or null.</returns>
        Task<T> GetById(long id);

        /// <summary>
        /// Get all elements with details.
        /// </summary>
        /// <param name="includeProperties">Name of properties which should be included.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{T}"/> that contains elements.</returns>
        Task<IEnumerable<T>> GetAllWithDetails(string includeProperties = "");

        /// <summary>
        /// Get elements by a specific filter.
        /// </summary>
        /// <param name="predicate">Filter with key.</param>
        /// <param name="includeProperties">Name of properties which should be included.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{T}"/> that contains elements.</returns>
        Task<IEnumerable<T>> GetByFilter(Expression<Func<T, bool>> predicate, string includeProperties = "");

        /// <summary>
        /// Get elements by a specific filter with no tracking.
        /// </summary>
        /// <param name="predicate">Filter with key.</param>
        /// <param name="includeProperties">Name of properties which should be included.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{T}"/> that contains elements.</returns>
        IQueryable<T> GetByFilterNoTracking(Expression<Func<T, bool>> predicate, string includeProperties = "");

        /// <summary>
        /// Get the amount of elements with filter or without it.
        /// </summary>
        /// <param name="where">Filter.</param>
        /// <returns>>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains an amount of found elements.</returns>
        Task<int> Count(Expression<Func<T, bool>> where = null);

        /// <summary>
        /// Get ordered, filtered list of elements.
        /// </summary>
        /// <typeparam name="TOrderKey">The type that we want to order list with.</typeparam>
        /// <param name="skip">How many records we want tp skip.</param>
        /// <param name="take">How many records we want to take.</param>
        /// <param name="includeProperties">What Properties we want to include to objects that we will receive.</param>
        /// <param name="where">Filter.</param>
        /// <param name="orderBy">Filter that defines by wich property we want to order by.</param>
        /// <param name="ascending">Ascending or descending ordering.</param>
        /// <returns>>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains an ordered, filtered <see cref="IQueryable{T}"/>.</returns>
        IQueryable<T> Get<TOrderKey>(int skip = 0, int take = 0, string includeProperties = "", Expression<Func<T, bool>> where = null, Expression<Func<T, TOrderKey>> orderBy = null, bool ascending = true);
    }
}