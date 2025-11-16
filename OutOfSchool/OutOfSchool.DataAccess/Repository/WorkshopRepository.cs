using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Util;

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
    public async Task<Workshop> GetWithNavigations(Guid id, bool asNoTracking = false)
    {
        IQueryable<Workshop> query = db.Workshops
            .Include(ws => ws.Teachers)
            .Include(ws => ws.DateTimeRanges)
            .Include(ws => ws.Images)
            .Include(ws => ws.Tags)
            .Include(ws => ws.WorkshopDescriptionItems.OrderBy(wdi => wdi.SectionName.ToLower()))
            .Include(ws => ws.Contacts).ThenInclude(c => c.Emails)
            .Include(ws => ws.Contacts).ThenInclude(c => c.Phones)
            .Include(ws => ws.Contacts).ThenInclude(c => c.SocialNetworks)
            .Include(ws => ws.Contacts)
            .ThenInclude(c => c.Address)
            .ThenInclude(a => a.CATOTTG)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent)
            .ThenInclude(c => c.Parent);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(ws => ws.Id == id && !ws.IsDeleted);
    }

    public async Task<IEnumerable<Workshop>> GetByIds(
        IEnumerable<Guid> ids,
        Func<IQueryable<Workshop>, IQueryable<Workshop>> includeExpression)
    {
        var query = includeExpression?.Invoke(dbSet) ?? dbSet;
        return await query.Where(w => ids.Contains(w.Id) && w.Status != WorkshopStatus.Archived).ToListAsync();
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

    public async Task<List<WorkshopPendingApplications>> AmountOfPendingApplications(List<Guid> workshopIds)
    {
        return await dbSet
            .Where(w => workshopIds.Contains(w.Id))
            .Select(w => new
            {
                WorkshopId = w.Id,
                PendingCount = w.Applications.Count(
                    a => a.Status == ApplicationStatus.Pending
                    && !a.IsDeleted
                    && a.Child != null
                    && !a.Child.IsDeleted
                    && a.Parent != null
                    && !a.Parent.IsDeleted)
            })
            .Where(w => w.PendingCount > 0)
            .Select(w => new WorkshopPendingApplications(w.WorkshopId, w.PendingCount))
            .ToListAsync();
    }
}
