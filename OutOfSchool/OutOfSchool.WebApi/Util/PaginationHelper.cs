using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.WebApi.Util
{
    /// <summary>
    /// Helper for pagination.
    /// </summary>
    /// <typeparam name="T">Entity type to get paginated results for.</typeparam>
    public class PaginationHelper<T> : IPaginationHelper<T>
        where T : class, new()
    {
        private readonly IEntityRepository<T> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationHelper{T}"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for entity wich list we want to paginate for.</param>
        public PaginationHelper(IEntityRepository<T> entityRepository)
        {
            repository = entityRepository;
        }

        /// <inheritdoc/>
        public async Task<int> GetCountOfPages(int pageSize, Expression<Func<T, bool>> where = null)
        {
            PageSizeValidation(pageSize);
            int listOfPages = await repository.Count(where).ConfigureAwait(false);
            int amountOfPages = (int)Math.Ceiling((double)(listOfPages / pageSize));
            return amountOfPages;
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetPage<TOrderKey>(int pageNumber, int pageSize, string includeProperties = "", Expression<Func<T, bool>> where = null, Expression<Func<T, TOrderKey>> orderBy = null, bool ascending = true)
        {
            PageSizeValidation(pageSize);
            int realAmount = await GetCountOfPages(pageSize).ConfigureAwait(false);
            PageNumberValidation(pageNumber, realAmount);
            var selectedPages = repository.Get<TOrderKey>(pageSize * (pageNumber - 1), pageSize, includeProperties, where, orderBy, ascending).ToList();
            return selectedPages;
        }

        private void PageSizeValidation(int pageSize)
        {
            if (pageSize < 1)
            {
                throw new ArgumentException("Wrong limit", nameof(pageSize));
            }
        }

        private void PageNumberValidation(int pageNumber, int realCount)
        {
            if (realCount < pageNumber)
            {
                throw new ArgumentException("Wrong page number", nameof(pageNumber));
            }
        }
    }
}
