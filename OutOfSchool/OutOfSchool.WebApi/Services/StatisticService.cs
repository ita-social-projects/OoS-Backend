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
    public class StatisticService
    {
        private readonly IEntityRepository<Application> applicationRepository;
        private readonly IEntityRepository<Workshop> workshopRepository;

        public StatisticService(
            IEntityRepository<Application> applicationRepository, 
            IEntityRepository<Workshop> workshopRepository)
        {
            this.applicationRepository = applicationRepository;
            this.workshopRepository = workshopRepository;
        }

        public async Task<IEnumerable<CategoryDTO>> GetPopularCategories(int number)
        {
            var workshops = await workshopRepository.GetAll().ConfigureAwait(false);

            var workshopsByCategory = workshops.GroupBy(w => w.Subsubcategory.Subcategory.Category)
                                                     .Select(g => new
                                                     {
                                                         Category = g.Key,
                                                         WorkshopsCount = g.Count(), 
                                                     });

            return workshopsByCategory.Select(g => g.Category).Select(c => c.ToModel());
        }

        public async Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int number)
        {
            var applications = await applicationRepository.GetAll().ConfigureAwait(false);

            var applicationGroups = applications.GroupBy(a => a.Workshop)
                                                .Select(g => new 
                                                {
                                                    Workshop = g.Key,
                                                    ApplicationsCount = g.Count(),
                                                });

            var sortedWorkshops = applicationGroups.OrderByDescending(q => q.ApplicationsCount)
                                                .Select(g => g.Workshop)
                                                .ToList();

            return sortedWorkshops.Take(number).Select(w => w.ToModel());
        }

        private async Task<int> GetApplicationsCountByWorkshop(WorkshopDTO workshop)
        {
            var applications = await applicationRepository.GetByFilter(a => a.WorkshopId == workshop.Id)
                                                          .ConfigureAwait(false);

            return applications.Count();
        }
    }
}
