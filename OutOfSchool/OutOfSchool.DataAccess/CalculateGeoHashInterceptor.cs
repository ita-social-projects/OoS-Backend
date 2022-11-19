using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H3Lib;
using H3Lib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.Services;

public class CalculateGeoHashInterceptor : ISaveChangesInterceptor
{
    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
    }

    public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        return result;
    }

    public ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(result);
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CalculateGeoHash(eventData.Context);
        return result;
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        CalculateGeoHash(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void CalculateGeoHash(DbContext context)
    {
        var states = new[]
        {
            EntityState.Added,
            EntityState.Modified,
        };

        var addresses = context.ChangeTracker.Entries<Address>().Where(x => states.Contains(x.State));

        foreach (var entry in addresses)
        {
            entry.CurrentValues["GeoHash"] =
                Api.GeoToH3(
                    default(GeoCoord).SetDegrees(
                        Convert.ToDecimal(entry.CurrentValues["Latitude"]),
                        Convert.ToDecimal(entry.CurrentValues["Longitude"])), GeoMathHelper.Resolution).Value;
        }
    }
}
