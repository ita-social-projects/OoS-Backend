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

        // Returns categories with 3 SQL queries, but doesn`t return categories without applications
        public async Task<IEnumerable<CategoryStatistic>> GetPopularCategoriesV2(int limit)
        {
            logger.Information("Getting popular categories started.");

            var workshops = workshopRepository.Get<int>();
            var applications = applicationRepository.Get<int>();

            var categoriesWithWorkshops = await workshops.GroupBy(w => w.CategoryId)
                                                         .Select(g => new
                                                         {
                                                             Category = g.Key,
                                                             WorkshopsCount = g.Count(),
                                                         })
                                                         .ToListAsync().ConfigureAwait(false);

            var categoriesWithApplications = applications.GroupBy(a => a.Workshop.CategoryId)
                                                         .Select(g => new
                                                         {
                                                             Category = g.Key,
                                                             ApplicationsCount = g.Count(),
                                                         });

            var popularCategories = await categoriesWithApplications.OrderByDescending(c => c.ApplicationsCount)
                                                                    .Take(limit)
                                                                    .ToListAsync().ConfigureAwait(false);

            var categoriesWithCounts = popularCategories.Select(c => new
            {
                Category = c.Category,
                ApplicationsCount = c.ApplicationsCount,
                WorkshopsCount = categoriesWithWorkshops.FirstOrDefault(ct => ct.Category == c.Category)
                                                        .WorkshopsCount,
            });

            var categoryIds = categoriesWithCounts.Select(c => c.Category);

            Expression<Func<Category, bool>> filter = w => categoryIds.Contains(w.Id);

            var categories = await categoryRepository.Get<int>(where: filter).ToListAsync().ConfigureAwait(false);

            List<CategoryStatistic> statistics = new List<CategoryStatistic>();

            foreach (var category in categoriesWithCounts)
            {
                var categoryStatistic = new CategoryStatistic
                {
                    Category = categories.FirstOrDefault(c => c.Id == category.Category).ToModel(),
                    ApplicationsCount = category.ApplicationsCount,
                    WorkshopsCount = category.WorkshopsCount,
                };

                statistics.Add(categoryStatistic);
            }

            logger.Information($"All {statistics.Count} records were successfully received");

            return statistics;
        }

        // Return Categories with 2 SQL queries per category

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryStatistic>> GetPopularCategoriesV1(int limit)
        {
            var categories = await categoryRepository.GetAll().ConfigureAwait(false);

            var categoriesWithWorkshops = categories.Select(c => new
            {
                Category = c,
                Workshops = GetWorkshopsByCategoryId(c.Id),
            });

            List<CategoryStatistic> statistics = new List<CategoryStatistic>();

            foreach (var category in categoriesWithWorkshops)
            {
                var applicationsCount = await category.Workshops.Select(w => w.Applications.Count)
                                                                .ToListAsync()
                                                                .ConfigureAwait(false);

                var statistic = new CategoryStatistic
                {
                    Category = category.Category.ToModel(),
                    WorkshopsCount = await category.Workshops.CountAsync().ConfigureAwait(false),
                    ApplicationsCount = applicationsCount.Sum(),
                };

                statistics.Add(statistic);
            }

            var popularCategories = statistics.OrderByDescending(s => s.ApplicationsCount)
                                              .Take(limit);

            return popularCategories;
        }

        // Return categories with 1 SQL query
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

        private IQueryable<Workshop> GetWorkshopsByCategoryId(long id)
        {
            Expression<Func<Workshop, bool>> filter = w => w.CategoryId == id;

            var workshops = workshopRepository.Get<int>(where: filter);

            return workshops;
        }
    }
}
