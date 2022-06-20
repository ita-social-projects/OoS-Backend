using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface ICodeficatorRepository : IEntityRepository<Codeficator>
    {
        /// <summary>
        /// Get elements pair values (Id, Name) by a specific filter.
        /// </summary>
        /// <param name="predicate">Filter with key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{KeyValuePair}"/> that contains elements' Id and Name.</returns>
        public Task<IEnumerable<KeyValuePair<long, string>>> GetNamesByFilter(Expression<Func<Codeficator, bool>> predicate);
    }
}