using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IApplicationRepository : IEntityRepositoryBase<Guid, Application>
    {
        Task<IEnumerable<Application>> Create(IEnumerable<Application> applications);

        Task<int> GetCountByWorkshop(Guid workshopId);
    }
}
