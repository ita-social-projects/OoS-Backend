using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IApplicationRepository : IEntityRepositoryBase<Guid, Application>
{
    Task<Application> Update(Application entity, Action<Application> onSaveChanges);

    Task<int> GetCountByWorkshop(Guid workshopId);

    Task<int> UpdateAllApprovedApplications();
}