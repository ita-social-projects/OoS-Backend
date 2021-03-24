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
    /// <typeparam name="T">Entity wich list we want to paginate for.</typeparam>
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
        public async Task<int> GetAmountOfPages(int watchSize, Expression<Func<T, bool>> where = null)
        {
            WatchSizeValidation(watchSize);
            int listOfPages = await repository.Count(where).ConfigureAwait(false);
            int amountOfPages = (int)Math.Ceiling((double)(listOfPages / watchSize));
            return amountOfPages;
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetPageExpanded<TOrderKey>(int pageNumber, int watchSize, string includeProperties = "", Expression<Func<T, bool>> where = null, Expression<Func<T, TOrderKey>> orderBy = null, bool ascending = true)
        {
            WatchSizeValidation(watchSize);
            int realAmount = await GetAmountOfPages(watchSize).ConfigureAwait(false);
            PageNumberValidation(pageNumber, realAmount);
            List<T> selectedPages = repository.Get<TOrderKey>(watchSize * (pageNumber - 1), watchSize, includeProperties, where, orderBy, ascending).ToList();
            return selectedPages;
        }

        private void WatchSizeValidation(int watchSize)
        {
            if (watchSize < 1)
            {
                throw new ArgumentException("Wrong limit", nameof(watchSize));
            }
        }

        private void PageNumberValidation(int pageNumber, int realAmount)
        {
            if (realAmount < pageNumber)
            {
                throw new ArgumentException("Wrong page number", nameof(pageNumber));
            }
        }
    }
}
