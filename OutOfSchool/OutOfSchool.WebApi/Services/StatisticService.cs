using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the operations to get popular workshops and categories.
    /// </summary>
    public class StatisticService : IStatisticService
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IWorkshopRepository workshopRepository;
        private readonly IEntityRepository<Category> categoryRepository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticService"/> class.
        /// </summary>
        /// <param name="applicationRepository">Application repository.</param>
        /// <param name="workshopRepository">Workshop repository.</param>
        /// <param name="categoryRepository">Category repository.</param>
        /// <param name="logger">Logger.</param>
        public StatisticService(
            IApplicationRepository applicationRepository,
            IWorkshopRepository workshopRepository,
            IEntityRepository<Category> categoryRepository,
            ILogger logger)
        {
            this.applicationRepository = applicationRepository;
            this.workshopRepository = workshopRepository;
            this.categoryRepository = categoryRepository;
            this.logger = logger;
        }

        public async Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int number)
        {
            logger.Information("Getting popular categories started.");

            var categories = await categoryRepository.GetAll().ConfigureAwait(false);

            var workshopsByCategories = categories.Select(c => new
            {
                Category = c,
                Workshops = GetWorkshopsByCategory(c.ToModel()),
            });

            List<CategoryStatistic> categoriesStatistics = new List<CategoryStatistic>();

            foreach (var group in workshopsByCategories)
            {
                var workshops = await group.Workshops.ToListAsync().ConfigureAwait(false);

                var category = new CategoryStatistic
                {
                    Category = group.Category.ToModel(),
                    WorkshopsCount = workshops.Count,
                    ApplicationsCount = 0,
                };

                var applicationsCounts = GetApplicationsCountAsync(workshops.Select(w => w.ToModel()));

                await foreach (var count in applicationsCounts)
                {
                    category.ApplicationsCount += count;
                }

                categoriesStatistics.Add(category);
            }

            var popularCategories = categoriesStatistics.OrderByDescending(c => c.ApplicationsCount).Take(number);

            logger.Information($"All {popularCategories.Count()} records were successfully received");

            return popularCategories;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int number)
        {
            logger.Information("Getting popular categories started.");

            var workshops = await workshopRepository.GetAll().ConfigureAwait(false);

            var applicationGroups = workshops.Select(async w => new
            {
                Workshop = w,
                ApplicationsCount = await applicationRepository.GetCountByWorkshop(w.Id).ConfigureAwait(false),
            }).Select(t => t.Result);

            var sortedWorkshops = applicationGroups.OrderByDescending(q => q.ApplicationsCount)
                                                   .Select(g => g.Workshop)
                                                   .ToList();

            var popularWorkshops = sortedWorkshops.Take(number).Select(w => w.ToModel());

            logger.Information($"All {popularWorkshops.Count()} records were successfully received");

            return popularWorkshops;
        }

        private async IAsyncEnumerable<int> GetApplicationsCountAsync(IEnumerable<WorkshopDTO> workshops)
        {
            foreach (var workshop in workshops)
            {
                var result = await applicationRepository.GetCountByWorkshop(workshop.Id).ConfigureAwait(false);

                yield return result;
            }
        }

        private IQueryable<Workshop> GetWorkshopsByCategory(CategoryDTO category)
        {
            Expression<Func<Workshop, bool>> filter = w => w.CategoryId == category.Id;

            var workshops = workshopRepository.Get<int>(where: filter);

            return workshops;
        } 
    }
}
