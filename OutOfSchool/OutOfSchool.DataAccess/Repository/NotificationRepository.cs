using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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