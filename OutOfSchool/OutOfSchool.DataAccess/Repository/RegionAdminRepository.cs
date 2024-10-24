using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class RegionAdminRepository : EntityRepositorySoftDeleted<(string, long), RegionAdmin>, IRegionAdminRepository
{
    private readonly OutOfSchoolDbContext db;

    public RegionAdminRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    public async Task<RegionAdmin> GetByIdAsync(string userId)
    {
        return await db.RegionAdmins
            .SingleOrDefaultAsync(ra => ra.UserId == userId);
    }
}