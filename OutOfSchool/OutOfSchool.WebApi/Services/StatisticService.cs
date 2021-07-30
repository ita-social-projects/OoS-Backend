using System;
using System.Collections.Generic;
using System.Linq;
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

        // Return categories with 1 SQL query

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int limit)
        {
            logger.Information("Getting popular categories started.");

            var workshops = workshopRepository.Get<int>();
            var applications = applicationRepository.Get<int>();

            var categoriesWithWorkshops = workshops.GroupBy(w => w.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    WorkshopsCount = g.Count() as int?,
                });

            var categoriesWithApplications = applications.GroupBy(a => a.Workshop.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    ApplicationsCount = g.Count() as int?,
                });

            // LEFT JOIN CategoriesWithWorkshops with CategoriesWithApplications
            var categoriesWithCounts = categoriesWithWorkshops
                .GroupJoin(
                categoriesWithApplications,
                categoryWithWorkshop => categoryWithWorkshop.CategoryId,
                categoryWithApplication => categoryWithApplication.CategoryId,
                (categoryWithWorkshop, categoriesWithApplications) => new
                {
                    categoryWithWorkshop,
                    categoriesWithApplications,
                })
                .SelectMany(
                x => x.categoriesWithApplications.DefaultIfEmpty(),
                (x, y) => new 
                {
                    CategoryId = x.categoryWithWorkshop.CategoryId,
                    ApplicationsCount = y.ApplicationsCount,
                    WorkshopsCount = x.categoryWithWorkshop.WorkshopsCount,
                });

            var allCategories = categoryRepository.Get<int>();

            // LEFT JOIN CategoriesWithCounts with all Categories
            var statistics = allCategories
                .GroupJoin(
                categoriesWithCounts,
                category => category.Id,
                categoryWithCounts => categoryWithCounts.CategoryId,
                (category, categoriesWithCounts) => new { category, categoriesWithCounts })
                .SelectMany(
                x => x.categoriesWithCounts.DefaultIfEmpty(),
                (x, y) => new CategoryStatistic
                {
                    Category = x.category.ToModel(),
                    ApplicationsCount = y.ApplicationsCount ?? 0,
                    WorkshopsCount = y.WorkshopsCount ?? 0,
                });

            var sortedStatistics = await statistics.OrderByDescending(s => s.ApplicationsCount)
                .Take(limit)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.Information($"All {sortedStatistics.Count} records were successfully received");

            return sortedStatistics;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int limit)
        {
            logger.Information("Getting popular workshops started.");

            var workshops = workshopRepository.Get<int>();

            var workshopsWithApplications = workshops.Select(w => new
            {
                Workshop = w,
                Applications = w.Applications.Count,
            });

            var popularWorkshops = workshopsWithApplications.OrderByDescending(w => w.Applications)
                                                            .Select(w => w.Workshop)
                                                            .Take(limit);

            var workshopDtos = await popularWorkshops.Select(w => w.ToModelSimple())
                                                     .ToListAsync().ConfigureAwait(false);

            logger.Information($"All {workshopDtos.Count} records were successfully received");

            return workshopDtos;
        }
    }
}
