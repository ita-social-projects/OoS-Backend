using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Workshop entity.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private readonly IWorkshopRepository repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IPaginationHelper<Workshop> paginationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Workshop entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public WorkshopService(IWorkshopRepository repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
            this.paginationHelper = new PaginationHelper<Workshop>(repository);
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            logger.Information("Teacher creating was started.");

            var workshop = dto.ToDomain();

            var newWorkshop = await repository.Create(workshop).ConfigureAwait(false);

            return newWorkshop.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetAll()
        {
            logger.Information("Process of getting all Workshops started.");

            var workshops = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!workshops.Any()
                ? "Workshop table is empty."
                : "Successfully got all records from the Workshop table.");

            return workshops.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> GetById(long id)
        {
            logger.Information("Process of getting Teacher by id started.");

            var teacher = await repository.GetById(id).ConfigureAwait(false);

            if (teacher == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Teacher with id = {id}.");

            return teacher.ToModel();
        }

        public async Task<IEnumerable<WorkshopDTO>> GetWorkshopsByOrganization(long id)
        {
            var workshops = await repository.GetByFilter(x => x.Provider.Id == id).ConfigureAwait(false);

            return workshops.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Update(WorkshopDTO dto)
        {
            logger.Information("Workshop updating was launched.");

            try
            {
                var workshop = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Workshop successfully updated.");

                return workshop.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Workshop in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Workshop deleting was launched.");

            var entity = new Workshop() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Workshop successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Teacher in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetPagesCount(WorkshopFilter filter, int size)
        {
            PaginationValidation(filter);
            var predicate = PredicateBuild(filter);
            return await paginationHelper.GetCountOfPages(size, predicate).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<List<WorkshopDTO>> GetPage(WorkshopFilter filter, int size, int pageNumber)
        {
            PaginationValidation(filter);
            var predicate = PredicateBuild(filter);
            
            bool ascending = (bool)filter.OrderByPriceAscending;
            Expression<Func<Workshop, decimal>> orderBy = x => x.Price;

            var page = await paginationHelper.GetPage(pageNumber, size, null, predicate, orderBy, ascending).ConfigureAwait(false);

            return page.Select(x => x.ToModel()).ToList();
        }

        private void PaginationValidation (WorkshopFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentException(localizer["The filter cannot be null"]);
            }
        }

        private Expression<Func<Workshop, bool>> PredicateBuild(WorkshopFilter filter)
        {
            var predicate = PredicateBuilder.True<Workshop>();

            if (!string.IsNullOrEmpty(filter.SearchFieldText))
            {
                predicate = predicate.And(x => x.Title.Contains(filter.SearchFieldText));
            }

            if (filter.Age != 0)
            {
                predicate = predicate.And(x => (x.MinAge <= filter.Age) && (x.MaxAge >= filter.Age));
            }

            if (filter.DaysPerWeek != 0)
            {
                predicate = predicate.And(x => x.DaysPerWeek == filter.DaysPerWeek);
            }

            predicate = predicate.And(x => x.Price >= filter.MinPrice);

            if (filter.MaxPrice != 0)
            {
                predicate = predicate.And(x => x.Price <= filter.MaxPrice);
            }

            predicate = predicate.And(x => x.WithDisabilityOptions == filter.Disability);

            if (filter.Categories != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var category in filter.Categories)
                {
                    tempPredicate = tempPredicate.Or(x => x.Category.Title == category);
                }

                predicate = predicate.And(tempPredicate);
            }

            if (filter.Subcategories != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var subcategory in filter.Subcategories)
                {
                    tempPredicate = tempPredicate.Or(x => x.Subcategory.Title == subcategory);
                }

                predicate = predicate.And(tempPredicate);
            }

            if (filter.Subsubcategories != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var subsubcategory in filter.Subsubcategories)
                {
                    tempPredicate = tempPredicate.Or(x => x.Subsubcategory.Title == subsubcategory);
                }

                predicate = predicate.And(tempPredicate);
            }

            return predicate;
        }
    }
}