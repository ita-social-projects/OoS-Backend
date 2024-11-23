using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.BaseEntities;

namespace OutOfSchool.Services;
public class TrackableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser currentUser;

    public TrackableEntityInterceptor(ICurrentUser currentUser)
    {
        this.currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        InitializeEntityMetadata(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        InitializeEntityMetadata(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SetCurrentValue<T>(EntityEntry entry, string propertyName, T newValue) =>
    entry.Property(propertyName).CurrentValue = newValue;

    private void InitializeEntityMetadata(DbContext context)
    {
        if (context == null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries<TrackableBaseEntity>();

        foreach (var entry in entries)
        {
            SetProperties(entry);
        }
    }

    private void SetProperties(EntityEntry entry)
    {
        var userId = currentUser?.UserId ?? string.Empty;
        var utcNow = DateTimeOffset.UtcNow;

        if (entry.State == EntityState.Added)
        {
            SetCurrentValue(entry, nameof(TrackableBaseEntity.CreatedAt), utcNow);
            SetCurrentValue(entry, nameof(TrackableBaseEntity.CreatedBy), userId);
        }
        else if (entry.State == EntityState.Modified)
        {
            SetCurrentValue(entry, nameof(TrackableBaseEntity.ModifiedAt), utcNow);
            SetCurrentValue(entry, nameof(TrackableBaseEntity.ModifiedBy), userId);
        }
    }
}