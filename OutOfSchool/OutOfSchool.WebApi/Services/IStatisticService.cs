using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IStatisticService
    {
        Task<IEnumerable<CategoryStatistic>> GetPopularCategories(int number);

        Task<IEnumerable<WorkshopDTO>> GetPopularWorkshops(int number);
    }
}
