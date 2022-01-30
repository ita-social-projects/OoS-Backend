using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEntityRepository<Notification> notificationRepository;
        private readonly ILogger<NotificationService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="notificationRepository">Repository for the Notification entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public NotificationService(
            IEntityRepository<Notification> notificationRepository,
            ILogger<NotificationService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            this.notificationRepository = notificationRepository;
            this.logger = logger;
            this.localizer = localizer;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<NotificationDto> Create(NotificationDto notificationDto)
        {
            logger.LogInformation("Notification creating was started.");

            var notification = mapper.Map<Notification>(notificationDto);

            var newNotification = await notificationRepository.Create(notification).ConfigureAwait(false);

            logger.LogInformation($"Notification with Id = {newNotification?.Id} created successfully.");

            return mapper.Map<NotificationDto>(newNotification);
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
        public async Task<IEnumerable<NotificationDto>> GetAllUsersNotificationsAsync(string userId)
        {
            logger.LogInformation("Getting all notifications for user started.");

            Expression<Func<Notification, bool>> filter = a => a.UserId == userId;

            var notifications = await notificationRepository.GetByFilter(filter).ConfigureAwait(false);

            logger.LogInformation(!notifications.Any()
                ? "Notification table is empty."
                : $"All {notifications.Count()} records were successfully received from the Notification table");

            return notifications.Select(notification => mapper.Map<NotificationDto>(notification)).ToList();
        }
    }
}
