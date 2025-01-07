using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Services.Util;

namespace OutOfSchool.Services.Repository.Api;

public interface IApplicationRepository : IEntityRepositorySoftDeleted<Guid, Application>
{
    Task<Application> Update(Application entity, Action<Application> onSaveChanges);

    Task<int> GetCountByWorkshop(Guid workshopId);

    Task<int> UpdateAllApprovedApplications();

    Task DeleteChildApplications(Guid childId);
    
    Task<List<WorkshopTakenSeats>> CountTakenSeatsForWorkshops(List<Guid> workshopIds);
}