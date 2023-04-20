using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class InstitutionAdminRepository : EntityRepository<(string, Guid), InstitutionAdmin>, IInstitutionAdminRepository
{
    private readonly OutOfSchoolDbContext db;

    public InstitutionAdminRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    public async Task<InstitutionAdmin> GetByIdAsync(string userId)
    {
        return await db.InstitutionAdmins
            .SingleOrDefaultAsync(pa => pa.UserId == userId);
    }
}