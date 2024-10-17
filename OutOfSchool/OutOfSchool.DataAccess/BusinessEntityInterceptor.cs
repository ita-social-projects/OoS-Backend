#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services;

public class BusinessEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser? currentUser;


    public BusinessEntityInterceptor(ICurrentUser? currentUser)
    {
        this.currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateBusinessEntities(eventData.Context, currentUser);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateBusinessEntities(eventData.Context, currentUser);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateBusinessEntities(
        DbContext context,
        ICurrentUser? currentUser)
    {
        var userId = currentUser?.UserId ?? string.Empty;

        var businessEntries = context.ChangeTracker.Entries()
            .Where(entry => entry.Entity is BusinessEntity);

        foreach (var entry in businessEntries)
        {
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                SetCurrentValue(entry, "CreatedAt", now);
                SetCurrentValue(entry, "CreatedBy", userId);
            }

            if (entry.State == EntityState.Modified)
            {
                SetCurrentValue(entry, "UpdatedAt", now);
                SetCurrentValue(entry, "ModifiedBy", userId);
            }

            // TODO: Any business entity currently is ISoftDeleted but not every ISoftDeleted is a business entity
            // TODO: so having this double check for now. Maybe there is a better solution?
            if (entry.State == EntityState.Deleted || (entry.CurrentValues["IsDeleted"] is true && entry.State == EntityState.Modified))
            {
                // This is a failsafe for cases that were not handled by similar check in repository
                if (entry.OriginalValues["IsSystemProtected"] is true)
                {
                    throw new InvalidOperationException("Cannot delete a protected object");
                }

                SetCurrentValue(entry, "DeleteDate", now);
                SetCurrentValue(entry, "DeletedBy", userId);
            }
        }
    }

    private static void SetCurrentValue<T>(
        EntityEntry entry,
        string propertyName,
        T newValue) =>
        entry.CurrentValues[propertyName] = newValue;
}