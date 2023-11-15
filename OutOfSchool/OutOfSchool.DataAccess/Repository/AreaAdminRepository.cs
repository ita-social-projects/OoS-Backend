using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

    public class AreaAdminRepository : EntityRepositorySoftDeleted<(string, long), AreaAdmin>, IAreaAdminRepository
    {
        private readonly OutOfSchoolDbContext db;

        public AreaAdminRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
        }

        public async Task<AreaAdmin> GetByIdAsync(string userId)
        {
            return await db.AreaAdmins
                .SingleOrDefaultAsync(ra => ra.UserId == userId);
        }
    }
