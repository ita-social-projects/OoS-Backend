using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

/// <summary>
/// Defines interface for CRUD functionality for Notification entity.
/// </summary>
public class NotificationRepository : SensitiveEntityRepository<Notification>, INotificationRepository
{
    private readonly OutOfSchoolDbContext db;

    public NotificationRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    /// <inheritdoc/>
    public async Task ClearNotifications()
    {
        // TODO: replace it in EFCore7:
        // await db.Notifications.Where(x => x.ReadDateTime == null && x.CreatedDateTime < dateNotReaded).ExecuteDeleteAsync();
        // await db.Notifications.Where(x => x.ReadDateTime != null && x.CreatedDateTime < dateReaded).ExecuteDeleteAsync();
        var dateNotReaded = DateTimeOffset.UtcNow.AddYears(-1);
        var dateReaded = DateTimeOffset.UtcNow.AddMonths(-1);

        await db.Database.ExecuteSqlRawAsync(
            @"DELETE FROM Notifications WHERE ReadDateTime IS NULL AND CreatedDateTime < {0};
            DELETE FROM Notifications WHERE ReadDateTime IS NOT NULL AND CreatedDateTime < {1};",
            dateNotReaded,
            dateReaded);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Notification>> SetReadDateTimeByType(string userId, NotificationType notificationType, DateTimeOffset dateTime)
    {
        var notifications = db.Notifications.Where(n => n.UserId == userId
                                                        && n.Type == notificationType
                                                        && n.ReadDateTime == null);
        await notifications.ForEachAsync(n => n.ReadDateTime = dateTime);

        await db.SaveChangesAsync();

        return await notifications.ToListAsync();
    }
}