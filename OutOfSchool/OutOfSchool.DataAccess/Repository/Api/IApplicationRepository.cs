using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IApplicationRepository : IEntityRepositorySoftDeleted<Guid, Application>
{
    Task<Application> Update(Application entity, Action<Application> onSaveChanges);

    Task<int> GetCountByWorkshop(Guid workshopId);

    Task<int> UpdateAllApprovedApplications();

    Task DeleteChildApplications(Guid childId);
}