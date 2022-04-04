using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Notifications;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
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
            logger.LogInformation("Notification creating was started.");

            var notification = mapper.Map<Notification>(notificationDto);
            notification.CreatedDateTime = DateTimeOffset.UtcNow;

            var newNotification = await notificationRepository.Create(notification).ConfigureAwait(false);

            logger.LogInformation($"Notification with Id = {newNotification?.Id} created successfully.");

            var notificationDtoReturn = mapper.Map<NotificationDto>(newNotification);

            return notificationDtoReturn;
        }

        public async Task Create(NotificationType type, NotificationAction action, Guid objectId, INotificationReciever service, Dictionary<string, string> additionalData = null)
        {
            if (!notificationsConfig.Value.Enabled || service is null)
            {
                return;
            }

            logger.LogInformation($"Notifications (type: {type}, action: {action}) creating was started.");

            var notification = new Notification()
            {
                Type = type,
                Action = action,
                CreatedDateTime = DateTimeOffset.UtcNow,
                ObjectId = objectId,
                Data = additionalData is null ? string.Empty : JsonConvert.SerializeObject(additionalData),
            };

            var recipients = await service.GetNotificationsRecipients(action, additionalData, objectId).ConfigureAwait(false);

            foreach (var user in recipients)
            {
                notification.Id = Guid.NewGuid();
                notification.UserId = user.Id;
                var newNotificationDto = await notificationRepository.Create(notification).ConfigureAwait(false);

                logger.LogInformation($"Notification with Id = {newNotificationDto?.Id} was created successfully.");

                await notificationHub.Clients
                    .Group(user.UserName)
                    .SendAsync("ReceiveNotification", JsonConvert.SerializeObject(newNotificationDto))
                    .ConfigureAwait(false);

                logger.LogInformation($"Notification with Id = {newNotificationDto?.Id} was sent to {user?.UserName} successfully.");
            }
        }

        /// <inheritdoc/>
        public async Task Delete(Guid id)
        {
            logger.LogInformation($"Deleting Notification with Id = {id} started.");

            var entity = new Notification() { Id = id };

            try
            {
                await notificationRepository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation($"Notification with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Deleting failed. Notification with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NotificationDto> Read(Guid id)
        {
            logger.LogInformation($"Updating ReadDateTime field in Notification with Id = {id} started.");

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

                logger.LogInformation($"Notification with Id = {id} updated succesfully.");

                return mapper.Map<NotificationDto>(result);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError($"Updating ReadDateTime in notification Id = {id} failed. Exception: {ex.Message}.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task ReadUsersNotificationsByType(string userId, NotificationType notificationType)
        {
            logger.LogInformation($"Updating ReadDateTime UserId = {userId} NotificationType = {notificationType} started.");

            try
            {
                var readDateTime = DateTimeOffset.UtcNow;
                _ = await notificationRepository.SetReadDateTimeByType(userId, notificationType, readDateTime).ConfigureAwait(false);

                logger.LogInformation($"Notifications UserId = {userId} NotificationType = {notificationType} updated succesfully.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError($"Updating ReadDateTime UserId = {userId} NotificationType = {notificationType} failed. Exception: {ex.Message}.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NotificationGroupedAndSingle> GetAllUsersNotificationsGroupedAsync(string userId)
        {
            logger.LogInformation($"Getting all notifications for user (userId = {userId}) started.");

            var allNotifications = await notificationRepository.GetByFilter(n => n.UserId == userId && n.ReadDateTime == null).ConfigureAwait(false);

            logger.LogInformation(!allNotifications.Any()
                ? $"Notification table for user (userId = {userId}) is empty."
                : $"All {allNotifications.Count()} records were successfully received from the Notification table for user (userId = {userId})");

            var result = new NotificationGroupedAndSingle()
            {
                NotificationsGrouped = new List<NotificationGrouped>(),
                Notifications = new List<NotificationDto>(),
            };

            List<NotificationType> grouped = new List<NotificationType>();

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
                        logger.LogInformation($"Error convert value '{item}' to type 'NotificationType'. Message: {ex.Message}");
                    }
                }
            }

            result.NotificationsGrouped = allNotifications
                .Where(n => grouped.Contains(n.Type))
                .GroupBy(n => n.Type)
                .Select(n => new NotificationGrouped { Type = n.Key, Amount = n.Count() })
                .ToList();

            result.Notifications = allNotifications
                .Where(n => !grouped.Contains(n.Type))
                .Select(notification => mapper.Map<NotificationDto>(notification))
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<NotificationDto>> GetAllUsersNotificationsByFilterAsync(string userId, NotificationType? notificationType)
        {
            logger.LogInformation($"Getting all notifications for user (userId = {userId}) started.");

            var filter = PredicateBuilder.True<Notification>();

            filter = filter.And(n => n.UserId == userId && n.ReadDateTime == null);

            if (!(notificationType is null))
            {
                filter = filter.And(n => n.Type == notificationType);
            }

            var notifications = await notificationRepository.GetByFilter(filter).ConfigureAwait(false);

            logger.LogInformation(!notifications.Any()
                ? $"Notification table for user (userId = {userId}) is empty."
                : $"All {notifications.Count()} records were successfully received from the Notification table for user (userId = {userId})");

            return notifications.Select(notification => mapper.Map<NotificationDto>(notification)).ToList();
        }

        /// <inheritdoc/>
        public async Task<int> GetAmountOfNewUsersNotificationsAsync(string userId)
        {
            logger.LogInformation($"Getting amount of new notifications for user (userId = {userId}) started.");

            var notifications = await notificationRepository.GetByFilter(n => n.UserId == userId && n.ReadDateTime == null).ConfigureAwait(false);

            logger.LogInformation(!notifications.Any()
                ? $"Notification table for user (userId = {userId}) is empty."
                : $"{notifications.Count()} records were successfully received from the Notification table for user (userId = {userId})");

            return notifications.Count();
        }

        /// <inheritdoc/>
        public async Task<NotificationDto> GetById(Guid id)
        {
            logger.LogInformation($"Getting Notification by Id started. Looking Id = {id}.");

            var notification = await notificationRepository.GetById(id).ConfigureAwait(false);

            if (notification is null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a Notification with Id = {id}.");

            return mapper.Map<NotificationDto>(notification);
        }
    }
}
