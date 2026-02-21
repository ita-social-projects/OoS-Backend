using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H3Lib;
using H3Lib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OutOfSchool.Common;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.Services;

public class CalculateGeoHashInterceptor : ISaveChangesInterceptor
{
    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        // we don't need to do anything
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
        _ = eventData ?? throw new ArgumentNullException(nameof(eventData));

        CalculateGeoHash(eventData.Context);
        return result;
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        _ = eventData ?? throw new ArgumentNullException(nameof(eventData));

        CalculateGeoHash(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void CalculateGeoHash(DbContext context)
    {
        var states = new[]
        {
            EntityState.Added,
            EntityState.Modified,
        };

        // TODO: This entity will stay until we fully move everything to unified contacts
        foreach (var entry in context.ChangeTracker.Entries<Address>().Where(x => states.Contains(x.State)).Select(x => x.CurrentValues))
        {
            entry["GeoHash"] =
                Api.GeoToH3(
                    default(GeoCoord).SetDegrees(
                        Convert.ToDecimal(entry["Latitude"]),
                        Convert.ToDecimal(entry["Longitude"])), GeoMathHelper.Resolution).Value;
        }

        foreach (var entry in context.ChangeTracker.Entries<ContactsAddress>().Where(x => states.Contains(x.State)).Select(x => x.CurrentValues))
        {
            entry["GeoHash"] =
                Api.GeoToH3(
                    default(GeoCoord).SetDegrees(
                        Convert.ToDecimal(entry["Latitude"]),
                        Convert.ToDecimal(entry["Longitude"])), GeoMathHelper.Resolution).Value;
        }
    }
}
