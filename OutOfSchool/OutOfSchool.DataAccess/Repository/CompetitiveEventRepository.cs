using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository;

public class CompetitiveEventRepository : EntityRepositorySoftDeleted<Guid, CompetitiveEvent>, ICompetitiveEventRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitiveEventRepository"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public CompetitiveEventRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IEnumerable<CompetitiveEvent>> GetByIds(IEnumerable<Guid> ids)
    {
        return await dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();
    }
}
