using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IAchievementRepository : IEntityRepositorySoftDeleted<Guid, Achievement>
{
    Task<IEnumerable<Achievement>> GetByWorkshopId(Guid workshopId);

    Task<Achievement> Create(Achievement achievement, List<Guid> childrenIDs, List<string> teachers);

    Task<Achievement> Update(Achievement achievement, List<Guid> childrenIDs, List<string> teachers);
}