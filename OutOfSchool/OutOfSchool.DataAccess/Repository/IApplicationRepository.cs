using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IApplicationRepository : IEntityRepository<Application>
    {
        Task<IEnumerable<Application>> Create(IEnumerable<Application> applications);

        Task<int> GetCountByWorkshop(long workshopId);
    }
}
