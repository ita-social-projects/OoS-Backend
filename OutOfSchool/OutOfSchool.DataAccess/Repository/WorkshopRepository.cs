using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class WorkshopRepository : SensitiveEntityRepositorySoftDeleted<Workshop>, IWorkshopRepository
{
    private readonly OutOfSchoolDbContext db;

    public WorkshopRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    /// <inheritdoc/>
    public new async Task Delete(Workshop entity)
    {
        db.Entry(entity).State = EntityState.Deleted;

        if (entity.Address != null)
        {
            db.Entry(entity.Address).State = EntityState.Deleted;
        }

        await db.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<Workshop> GetWithNavigations(Guid id, bool asNoTracking = false)
    {
        IQueryable<Workshop> query = db.Workshops
            .Include(ws => ws.Address)
            .Include(ws => ws.Teachers)
            .Include(ws => ws.DateTimeRanges)
            .Include(ws => ws.Images)
            .Include(ws => ws.Tags);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(ws => ws.Id == id && !ws.IsDeleted);
    }

    public async Task<IEnumerable<Workshop>> GetByIds(IEnumerable<Guid> ids)
    {
        return await dbSet.Where(w => ids.Contains(w.Id)).ToListAsync();
    }

    public async Task<IEnumerable<Workshop>> UpdateProviderTitle(Guid providerId, string providerTitle, string providerTitleEn)
    {
        var workshops = db.Workshops.Where(ws => ws.ProviderId == providerId);

        await workshops.ExecuteUpdateAsync(settter => settter
                .SetProperty(ws => ws.ProviderTitle, providerTitle)
                .SetProperty(ws => ws.ProviderTitleEn, providerTitleEn))
            .ConfigureAwait(false);

        await db.SaveChangesAsync();

        return await workshops.ToListAsync();
    }

    public async Task<IEnumerable<Workshop>> BlockByProvider(Provider provider)
    {
        var workshops = db.Workshops.Where(ws => ws.ProviderId == provider.Id);
        await workshops.ForEachAsync(ws =>
        {
            ws.IsBlocked = provider.IsBlocked;
        });

        await db.SaveChangesAsync();

        return await workshops.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<uint> GetAvailableSeats(Guid workshopId)
    {
        return await db.Workshops.Where(w => w.Id == workshopId).Select(x => x.AvailableSeats).FirstAsync();
    }

    public override async Task<Workshop> Create(Workshop workshop)
    {
        await dbSet.AddAsync(workshop).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return await Task.FromResult(workshop).ConfigureAwait(false);
    }

    public Task<List<Workshop>> GetAllWithDeleted(Expression<Func<Workshop, bool>> whereExpression)
    {
        IQueryable<Workshop> query = db.Workshops;

        if (whereExpression != null)
        {
            query = query.Where(whereExpression);
        }

        return query.ToListAsync();
    }
}
