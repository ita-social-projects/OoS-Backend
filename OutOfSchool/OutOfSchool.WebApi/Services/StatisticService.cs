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

        // TOO MANY SQL QUERIES
        //public async Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int limit)
        //{
        //    logger.Information("Getting popular categories started.");

        //    var categories = await categoryRepository.GetAll().ConfigureAwait(false);

        //    var workshopsByCategories = categories.Select(c => new
        //    {
        //        Category = c,
        //        Workshops = GetWorkshopsByCategory(c.ToModel()),
        //    });

        //    List<CategoryStatistic> categoriesStatistics = new List<CategoryStatistic>();

        //    foreach (var group in workshopsByCategories)
        //    {
        //        var workshops = await group.Workshops.ToListAsync().ConfigureAwait(false);

        //        var category = new CategoryStatistic
        //        {
        //            Category = group.Category.ToModel(),
        //            WorkshopsCount = workshops.Count,
        //            ApplicationsCount = 0,
        //        };

        //        var applicationsCounts = GetApplicationsCountAsync(workshops.Select(w => w.ToModel()));

        //        await foreach (var count in applicationsCounts)
        //        {
        //            category.ApplicationsCount += count;
        //        }

        //        categoriesStatistics.Add(category);
        //    }

        //    var popularCategories = categoriesStatistics.OrderByDescending(c => c.ApplicationsCount).Take(limit);

        //    logger.Information($"All {popularCategories.Count()} records were successfully received");

        //    return popularCategories;
        //}

        // NOT TRANSLATED
        //public async Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int limit)
        //{
        //    var categories = categoryRepository.Get<int>();

        //    var categoriesWithWorkshops = categories.Select(c => new
        //    {
        //        Category = c,
        //        Workshops = GetWorkshopsByCategoryId(c.Id).ToList(),
        //    });

        //    var categoriesWithCount = categoriesWithWorkshops.Select(c => new
        //    {
        //        Category = c.Category,
        //        WorkshopsCount = c.Workshops.Count,
        //        ApplicationsCount = GetApplicationsCount(c.Workshops),
        //    });

        //    var popularCategories = await categoriesWithCount.OrderByDescending(c => c.ApplicationsCount)
        //                                                     .Take(limit)
        //                                                     .ToListAsync().ConfigureAwait(false);

        //    var categoryStatistics = popularCategories.Select(c => new CategoryStatistic
        //    {
        //        Category = c.Category.ToModel(),
        //        ApplicationsCount = c.ApplicationsCount,
        //        WorkshopsCount = c.WorkshopsCount,
        //    });

        //    return categoryStatistics;
        //}

        // NOT WORKING
        //public async Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int limit)
        //{
        //    var workshops = workshopRepository.Get<int>();

        //    var categoriesWithWorkshops = workshops.GroupBy(w => w.CategoryId)
        //                                           .Select(w => new
        //                                           {
        //                                               Category = w.Key,
        //                                               WorkshopsCount = w.Count(),
        //                                               ApplicationsCount = ,
        //                                           });

        //    var popularCategories = await categoriesWithWorkshops.OrderByDescending(c => c.ApplicationsCount)
        //                                                         .Take(limit)
        //                                                         .ToListAsync().ConfigureAwait(false);

        //    var categories = categoryRepository.Get<int>();

        //    var categoryStatistics = popularCategories.Select(c => new CategoryStatistic
        //    {
        //        Category = categories.FirstOrDefault(ca => ca.Id == c.Category).ToModel(),
        //        WorkshopsCount = c.WorkshopsCount,
        //        ApplicationsCount = c.ApplicationsCount,
        //    });

        //    return categoryStatistics;
        //}

        public async Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int limit)
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

        // TOO MANY SQL QUERIES
        /// <inheritdoc/>
        //public async Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int limit)
        //{
        //    logger.Information("Getting popular workshops started.");

        //    var workshops = await workshopRepository.GetAll().ConfigureAwait(false);

        //    var applicationGroups = workshops.Select(async w => new
        //    {
        //        Workshop = w,
        //        ApplicationsCount = await applicationRepository.GetCountByWorkshop(w.Id).ConfigureAwait(false),
        //    }).Select(t => t.Result);

        //    var sortedWorkshops = applicationGroups.OrderByDescending(q => q.ApplicationsCount)
        //                                           .Select(g => g.Workshop)
        //                                           .ToList();

        //    var popularWorkshops = sortedWorkshops.Take(limit).Select(w => w.ToModel());

        //    logger.Information($"All {popularWorkshops.Count()} records were successfully received");

        //    return popularWorkshops;
        //}

        // NOT RETURN WORKSHOPS WITHOUT APPLICATIONS
        //public async Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int limit)
        //{
        //    var applications = applicationRepository.Get<int>();

        //    var applicationGroups = applications.GroupBy(a => a.WorkshopId)
        //                                        .Select(a => new
        //                                        {
        //                                            Workshop = a.Key,
        //                                            ApplicationsCount = a.Count(),
        //                                        });

        //    var popularWorkshops = applicationGroups.OrderByDescending(a => a.ApplicationsCount)
        //                                            .Select(a => a.Workshop)
        //                                            .Take(limit);

        //    Expression<Func<Workshop, bool>> filter = w => popularWorkshops.Contains(w.Id);

        //    var workshops = await workshopRepository.Get<int>(where: filter).ToListAsync().ConfigureAwait(false);

        //    return workshops.Select(w => w.ToModel());
        //}

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

        // NOT TRANSLATED
        //public async Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int limit)
        //{
        //    var workshops = workshopRepository.Get<int>().ToList();
        //    var applications = applicationRepository.Get<int>().ToList();

        //    var workshopsWithApplications = workshops.GroupJoin(
        //        applications,
        //        w => w.Id,
        //        ap => ap.WorkshopId,
        //        (w, aps) => new 
        //        {
        //            Workshop = w,
        //            ApplicationsCount = aps.Count(),
        //        });

        //    var popularWorkshops = workshopsWithApplications.OrderByDescending(w => w.ApplicationsCount)
        //                                                    .Select(w => w.Workshop)
        //                                                    .Take(limit);

        //    var workshopDtos = popularWorkshops.Select(w => w.ToModel());

        //    return workshopDtos;
        //}
    }
}
