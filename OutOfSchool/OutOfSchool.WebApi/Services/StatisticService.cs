using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the operations to get popular workshops and categories.
    /// </summary>
    public class StatisticService : IStatisticService
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IEntityRepository<Workshop> workshopRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticService"/> class.
        /// </summary>
        /// <param name="applicationRepository">Application repository.</param>
        /// <param name="workshopRepository">Workshop repository.</param>
        public StatisticService(
            IApplicationRepository applicationRepository, 
            IEntityRepository<Workshop> workshopRepository)
        {
            this.applicationRepository = applicationRepository;
            this.workshopRepository = workshopRepository;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int number)
        {
            var workshops = await workshopRepository.GetAll().ConfigureAwait(false);

            var workshopGroups = workshops.GroupBy(w => w.Subsubcategory.Subcategory.Category)
                                          .Select(g => new
                                          {
                                              Category = g.Key,
                                              WorkshopsCount = g.Count(),
                                              ApplicationsCounts = GetApplicationsCountAsync(g.Select(g => g.ToModel())),
                                          });

            List<CategoryStatistic> categories = new List<CategoryStatistic>();

            foreach (var group in workshopGroups)
            {
                var category = new CategoryStatistic
                {
                    Category = group.Category.ToModel(),
                    WorkshopsCount = group.WorkshopsCount,
                    ApplicationsCount = 0,
                };

                await foreach (var count in group.ApplicationsCounts)
                {
                    category.ApplicationsCount += count;
                }

                categories.Add(category);
            }

            return categories.OrderByDescending(c => c.ApplicationsCount).Take(number);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int number)
        {
            var applications = await applicationRepository.GetAll().ConfigureAwait(false);

            var workshops = await workshopRepository.GetAll().ConfigureAwait(false);

            var applicationGroups = workshops.Select(async w => new 
                                             {
                                                 Workshop = w,
                                                 ApplicationsCount = await applicationRepository.GetCountByWorkshop(w.Id)
                                                                                                .ConfigureAwait(false),
                                             })
                                             .Select(t => t.Result);

            var sortedWorkshops = applicationGroups.OrderByDescending(q => q.ApplicationsCount)
                                                   .Select(g => g.Workshop)
                                                   .ToList();

            return sortedWorkshops.Take(number).Select(w => w.ToModel());
        }

        private async IAsyncEnumerable<int> GetApplicationsCountAsync(IEnumerable<WorkshopDTO> workshops)
        {
            foreach (var workshop in workshops)
            {
                var result = await applicationRepository.GetCountByWorkshop(workshop.Id).ConfigureAwait(false);

                yield return result;
            }
        }
    }
}
