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
        private readonly IEntityRepository<Direction> directionRepository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticService"/> class.
        /// </summary>
        /// <param name="applicationRepository">Application repository.</param>
        /// <param name="workshopRepository">Workshop repository.</param>
        /// <param name="directionRepository">Direction repository.</param>
        /// <param name="logger">Logger.</param>
        public StatisticService(
            IApplicationRepository applicationRepository,
            IWorkshopRepository workshopRepository,
            IEntityRepository<Direction> directionRepository,
            ILogger logger)
        {
            this.applicationRepository = applicationRepository;
            this.workshopRepository = workshopRepository;
            this.directionRepository = directionRepository;
            this.logger = logger;
        }

        // Return categories with 1 SQL query

        /// <inheritdoc/>
        public async Task<IEnumerable<DirectionStatistic>> GetPopularDirections(int limit)
        {
            logger.Information("Getting popular categories started.");

            var workshops = workshopRepository.Get<int>();
            var applications = applicationRepository.Get<int>();

            var directionsWithWorkshops = workshops.GroupBy(w => w.DirectionId)
                .Select(g => new
                {
                    DirectionId = g.Key,
                    WorkshopsCount = g.Count() as int?,
                });

            var directionsWithApplications = applications.GroupBy(a => a.Workshop.DirectionId)
                .Select(g => new
                {
                    DirectionId = g.Key,
                    ApplicationsCount = g.Count() as int?,
                });

            // LEFT JOIN CategoriesWithWorkshops with CategoriesWithApplications
            var directionsWithCounts = directionsWithWorkshops
                .GroupJoin(
                directionsWithApplications,
                directionWithWorkshop => directionWithWorkshop.DirectionId,
                directionWithApplication => directionWithApplication.DirectionId,
                (directionWithWorkshop, directionsWithApplications) => new
                {
                    directionWithWorkshop,
                    directionsWithApplications,
                })
                .SelectMany(
                x => x.directionsWithApplications.DefaultIfEmpty(),
                (x, y) => new
                {
                    DirectionId = x.directionWithWorkshop.DirectionId,
                    ApplicationsCount = y.ApplicationsCount,
                    WorkshopsCount = x.directionWithWorkshop.WorkshopsCount,
                });

            var allDirections = directionRepository.Get<int>();

            // LEFT JOIN CategoriesWithCounts with all Categories
            var statistics = allDirections
                .GroupJoin(
                directionsWithCounts,
                direction => direction.Id,
                directionWithCounts => directionWithCounts.DirectionId,
                (direction, directionsWithCounts) => new { direction, directionsWithCounts })
                .SelectMany(
                x => x.directionsWithCounts.DefaultIfEmpty(),
                (x, y) => new DirectionStatistic
                {
                    Direction = x.direction.ToModel(),
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
        public async Task<IEnumerable<WorkshopCard>> GetPopularWorkshops(int limit)
        {
            logger.Information("Getting popular workshops started.");

            var workshops = workshopRepository.Get<int>(includeProperties: $"{nameof(Address)},{nameof(Direction)}");

            var workshopsWithApplications = workshops.Select(w => new
            {
                Workshop = w,
                Applications = w.Applications.Count,
            });

            var popularWorkshops = workshopsWithApplications.OrderByDescending(w => w.Applications)
                                                            .Select(w => w.Workshop)
                                                            .Take(limit);

            var popularWorkshopsList = await popularWorkshops.ToListAsync().ConfigureAwait(false);

            logger.Information($"All {popularWorkshopsList.Count} records were successfully received");

            return popularWorkshopsList.Select(w => w.ToCard());
        }
    }
}
