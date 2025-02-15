using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface ICompetitiveEventRepository : IEntityRepositorySoftDeleted<Guid, CompetitiveEvent>
{
    Task<IEnumerable<CompetitiveEvent>> GetByIds(IEnumerable<Guid> ids);
}
