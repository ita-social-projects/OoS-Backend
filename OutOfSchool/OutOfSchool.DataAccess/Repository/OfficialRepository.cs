using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

/// <inheritdoc cref="IOfficialRepository"/>
public class OfficialRepository : SensitiveEntityRepositorySoftDeleted<Official>, IOfficialRepository
{
    public OfficialRepository(OutOfSchoolDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<Guid> GetProviderIdByOfficialUserIdAsync(string userId) => await dbSet
        .Where(o => !o.IsDeleted && o.Individual.UserId == userId)
        .Include(o => o.Position)
        .Select(o => o.Position.ProviderId)
        .FirstOrDefaultAsync()
        .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<List<string>> GetActiveOfficialUserIdsByProviderId(Guid providerId) => await dbSet
        .Where(o => !o.IsDeleted && o.Position.ProviderId == providerId && o.Individual.UserId != null)
        .Include(o => o.Individual)
        .Select(o => o.Individual.UserId)
        .ToListAsync()
        .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<string> GetDirectorOfficialUserIdByProviderIdAsync(Guid providerId) => await dbSet
        .Where(o =>
            !o.IsDeleted && o.Position.ProviderId == providerId && o.Position.PositionType == PositionType.Director)
        .Include(o => o.Individual)
        .Select(o => o.Individual.UserId)
        .SingleOrDefaultAsync()
        .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Official> GetByUserIdAsync(string userId) => await dbSet
        .Where(o => !o.IsDeleted && o.Individual.UserId == userId)
        .Include(o => o.Position)
        .Include(o => o.Individual)
        .FirstOrDefaultAsync()
        .ConfigureAwait(false);
}