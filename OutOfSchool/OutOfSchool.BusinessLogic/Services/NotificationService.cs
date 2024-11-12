using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models.Notifications;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository notificationRepository;
    private readonly ILogger<NotificationService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly IHubContext<NotificationHub> notificationHub;
    private readonly IOptions<NotificationsConfig> notificationsConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="notificationRepository">Repository for the Notification entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="notificationHub">NotificationHub.</param>
    /// <param name="notificationsConfig">NotificationsConfig.</param>
    public NotificationService(
        INotificationRepository notificationRepository,
        ILogger<NotificationService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        IHubContext<NotificationHub> notificationHub,
        IOptions<NotificationsConfig> notificationsConfig)
    {
        this.notificationRepository = notificationRepository;
        this.logger = logger;
        this.localizer = localizer;
        this.mapper = mapper;
        this.notificationHub = notificationHub;
        this.notificationsConfig = notificationsConfig;
    }

    /// <inheritdoc/>
    public async Task<NotificationDto> Create(NotificationDto notificationDto)
    {
        logger.LogInformation("Notification creation started");

        var notification = mapper.Map<Notification>(notificationDto);
        notification.CreatedDateTime = DateTimeOffset.UtcNow;

        var newNotification = await notificationRepository.Create(notification).ConfigureAwait(false);

        logger.LogInformation("Notification with Id = {Id} created successfully", newNotification?.Id);

        var notificationDtoReturn = mapper.Map<NotificationDto>(newNotification);

        var unreadNotificationsCount = await GetAmountOfNewUsersNotificationsAsync(notification.UserId);

        await notificationHub.Clients
            .Group(notification.UserId)
            .SendAsync("ReceiveNotification", JsonSerializerHelper.Serialize(new { unreadNotificationsCount, notificationDtoReturn }))
            .ConfigureAwait(false);

        return notificationDtoReturn;
    }

    public async Task Create(
        NotificationType type,
        NotificationAction action,
        Guid objectId,
        IEnumerable<string> recipientsIds,
        Dictionary<string, string> additionalData = null,
        string groupedData = null)
    {
        if (!notificationsConfig.Value.Enabled || recipientsIds is null)
        {
            return;
        }

        logger.LogInformation("Notifications (type: {Type}, action: {Action}) creating was started", type, action);

        var notification = new Notification
        {
            Type = type,
            Action = action,
            CreatedDateTime = DateTimeOffset.UtcNow,
            ObjectId = objectId,
            Data = additionalData,
            GroupedData = groupedData,
        };

        foreach (var userId in recipientsIds)
        {
            notification.Id = Guid.NewGuid();
            notification.UserId = userId;
            var newNotificationDto = await notificationRepository.Create(notification).ConfigureAwait(false);

            logger.LogInformation("Notification with Id = {Id} was created successfully", newNotificationDto?.Id);

            var unreadNotificationsCount = await GetAmountOfNewUsersNotificationsAsync(notification.UserId);

            await notificationHub.Clients
                .Group(notification.UserId)
                .SendAsync("ReceiveNotification", JsonSerializerHelper.Serialize(new { unreadNotificationsCount, newNotificationDto }))
                .ConfigureAwait(false);

            logger.LogInformation(
                "Notification with Id = {Id} was sent to {UserId} successfully",
                newNotificationDto?.Id,
                userId);
        }
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogInformation("Deleting Notification with Id = {Id} started", id);

        var entity = await notificationRepository.GetById(id);

        try
        {
            await notificationRepository.Delete(entity).ConfigureAwait(false);

            logger.LogInformation("Notification with Id = {Id} successfully deleted", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Deleting failed. Notification with Id = {Id} doesn't exist in the system", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<NotificationDto> Read(Guid id)
    {
        logger.LogInformation("Updating ReadDateTime field in Notification with Id = {Id} started", id);

        try
        {
            var notification = await notificationRepository.GetById(id).ConfigureAwait(false);

            if (notification is null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer[$"Notification with Id = {id} doesn't exist in the system."]);
            }

            notification.ReadDateTime = DateTimeOffset.UtcNow;

            var result = await notificationRepository.Update(notification).ConfigureAwait(false);

            logger.LogInformation("Notification with Id = {Id} updated successfully", id);

            return mapper.Map<NotificationDto>(result);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Updating ReadDateTime in notification Id = {Id} failed", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ReadAll(string userId)
    {
        logger.LogInformation("Updating ReadDateTime for UserId = {UserId} all unreaded notifications started", userId);

        try
        {
            var readDateTime = DateTimeOffset.UtcNow;
            await notificationRepository.SetReadDateTimeForAllUnreaded(userId, readDateTime).ConfigureAwait(false);

            logger.LogInformation("All unreaded notifications for UserId = {UserId} updated successfully", userId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Updating ReadDateTime for UserId = {UserId} all unreaded notifications failed", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ReadUsersNotificationsByType(string userId, NotificationType notificationType)
    {
        logger.LogInformation("Updating ReadDateTime UserId = {UserId} NotificationType = {Type} started", userId, notificationType);

        try
        {
            var readDateTime = DateTimeOffset.UtcNow;
            _ = await notificationRepository.SetReadDateTimeByType(userId, notificationType, readDateTime).ConfigureAwait(false);

            logger.LogInformation("Notifications UserId = {UserId} NotificationType = {Type} updated successfully", userId, notificationType);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Updating ReadDateTime UserId = {UserId} NotificationType = {Type} failed", userId, notificationType);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<NotificationGroupedAndSingle> GetAllUsersNotificationsGroupedAsync(string userId)
    {
        logger.LogInformation("Getting all notifications for user (userId = {UserId}) started", userId);

        var allNotifications = (await notificationRepository.GetByFilter(n => n.UserId == userId && n.ReadDateTime == null)).ToList();

        logger.LogInformation(
            "{Count} records were successfully received from the Notification table for user (userId = {UserId})",
            allNotifications.Count,
            userId);

        var result = new NotificationGroupedAndSingle()
        {
            NotificationsGroupedByType = new List<NotificationGroupedByType>(),
            Notifications = new List<NotificationDto>(),
        };

        var grouped = new List<NotificationType>();

        if (allNotifications.Any())
        {
            foreach (var item in notificationsConfig.Value.Grouped)
            {
                try
                {
                    grouped.Add((NotificationType)Enum.Parse(typeof(NotificationType), item));
                }
                catch (ArgumentException ex)
                {
                    logger.LogInformation(ex, "Error convert value '{Item}' to type 'NotificationType'", item);
                }
            }
        }

        var notificationsGroupedByAdditionalData = allNotifications
            .Where(n => grouped.Contains(n.Type))
            .GroupBy(n => new { n.Type, n.Action, n.GroupedData })
            .Select(n => new NotificationGrouped
            {
                Type = n.Key.Type,
                Action = n.Key.Action,
                GroupedData = n.Key.GroupedData,
                Amount = n.Count(),
            })
            .ToList();

        result.NotificationsGroupedByType = notificationsGroupedByAdditionalData
            .GroupBy(n => n.Type)
            .Select(n => new NotificationGroupedByType()
            {
                Type = n.Key,
                Amount = n.Sum(n => n.Amount),
                GroupedByAdditionalData = n.ToList(),
            })
            .ToList();

        result.Notifications = allNotifications
            .Where(n => !grouped.Contains(n.Type))
            .Select(notification => mapper.Map<NotificationDto>(notification))
            .OrderByDescending(n => n.CreatedDateTime)
            .ToList();

        return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<NotificationDto>> GetAllUsersNotificationsByFilterAsync(string userId, NotificationType? notificationType)
    {
        logger.LogInformation("Getting all notifications for user (userId = {UserId}) started", userId);

        var filter = PredicateBuilder.True<Notification>();

        filter = filter.And(n => n.UserId == userId && n.ReadDateTime == null);

        if (notificationType is not null)
        {
            filter = filter.And(n => n.Type == notificationType);
        }

        var notifications = (await notificationRepository.GetByFilter(filter)).ToList();

        logger.LogInformation(
            "{Count} records were successfully received from the Notification table for user (userId = {UserId})",
            notifications.Count,
            userId);

        return notifications
            .Select(notification => mapper.Map<NotificationDto>(notification))
            .OrderByDescending(n => n.CreatedDateTime)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<int> GetAmountOfNewUsersNotificationsAsync(string userId)
    {
        logger.LogInformation("Getting amount of new notifications for user (userId = {UserId}) started", userId);

        var notifications = await notificationRepository.GetByFilter(n => n.UserId == userId && n.ReadDateTime == null).ConfigureAwait(false);

        var count = notifications.Count();

        logger.LogInformation("{Count} records were successfully received from the Notification table for user (userId = {UserId})", count, userId);

        return count;
    }

    /// <inheritdoc/>
    public async Task<NotificationDto> GetById(Guid id)
    {
        logger.LogInformation("Getting Notification by Id started. Looking Id = {Id}", id);

        var notification = await notificationRepository.GetById(id).ConfigureAwait(false);

        if (notification is null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation("Successfully got a Notification with Id = {Id}", id);

        return mapper.Map<NotificationDto>(notification);
    }
}