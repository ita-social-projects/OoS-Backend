#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services;

public class BusinessEntityInterceptor(in IServiceProvider provider) : SaveChangesInterceptor
{
    private readonly IServiceProvider serviceProvider = provider;


    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateBusinessEntities(eventData.Context, serviceProvider);
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
            UpdateBusinessEntities(eventData.Context, serviceProvider);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateBusinessEntities(
        DbContext context,
        IServiceProvider provider)
    {
        var currentUser = provider.GetRequiredService<ICurrentUser>();
        var userId = currentUser.UserId;

        var businessEntries = context.ChangeTracker.Entries()
            .Where(entity => entity.GetType().IsSubclassOf(typeof(BusinessEntity)));

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