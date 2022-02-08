using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ISensitiveEntityRepository<Notification> notificationRepository;
        private readonly ILogger<NotificationService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;
        private readonly IHubContext<NotificationHub> notificationHub;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="notificationRepository">Repository for the Notification entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        /// <param name="notificationHub">NotificationHub.</param>
        public NotificationService(
            ISensitiveEntityRepository<Notification> notificationRepository,
            ILogger<NotificationService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            IHubContext<NotificationHub> notificationHub)
        {
            this.notificationRepository = notificationRepository;
            this.logger = logger;
            this.localizer = localizer;
            this.mapper = mapper;
            this.notificationHub = notificationHub;
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

        public async Task Create(NotificationType type, NotificationAction action, Guid objectId, IEnumerable<User> notificationRecipients)
        {
            logger.LogInformation($"Notifications (type: {type}, action: {action}) creating was started.");

            var notification = new Notification()
            {
                Id = Guid.NewGuid(),
                Type = type,
                Action = action,
                CreatedDateTime = DateTimeOffset.UtcNow,
                ObjectId = objectId,
                Text = string.Empty,
            };

            foreach (var user in notificationRecipients)
            {
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
        public async Task<IEnumerable<NotificationDto>> GetAllUsersNotificationsByFilterAsync(string userId, NotificationType? notificationType)
        {
            logger.LogInformation($"Getting all notifications for user (userId = {userId}) started.");

            var filter = PredicateBuilder.True<Notification>();

            filter = filter.And(n => n.UserId == userId);

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

            var filter = PredicateBuilder.True<Notification>();

            filter = filter.And(n => n.UserId == userId);

            var notifications = await notificationRepository.GetByFilter(filter).ConfigureAwait(false);

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
