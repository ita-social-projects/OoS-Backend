using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService notificationService;
        private readonly ILogger<NotificationController> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="notificationService">Service for Notification model.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.notificationService = notificationService;
            this.logger = logger;
            this.localizer = localizer;
        }

        /// <summary>
        /// Add a new Notification to the database.
        /// </summary>
        /// <param name="notificationDto">Notification entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        /// <response code="201">Notification was successfully created.</response>
        /// <response code="400">NotificationDto was wrong.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(NotificationDto notificationDto)
        {
            var notification = await notificationService.Create(notificationDto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = notification.Id, },
                notification);
        }

        /// <summary>
        /// Delete the Notification entity from DB.
        /// </summary>
        /// <param name="id">The key of the Notification in table.</param>
        /// <returns>Status Code.</returns>
        /// <response code="204">Notification was successfully deleted.</response>
        /// <response code="400">Id was wrong.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(Guid id)
        {
            await notificationService.Delete(id).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Update ReadDateTime field in Notification.
        /// </summary>
        /// <param name="id">The key of the Notification in table.</param>
        /// <returns>Status Code.</returns>
        /// <response code="200">Notification was successfully updated.</response>
        /// <response code="400">Id was wrong.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Read(Guid id)
        {
            return Ok(await notificationService.Read(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Update ReadDateTime field in all notifications with specified notificationType.
        /// </summary>
        /// <param name="notificationType">NotificationType.</param>
        /// <returns>Status Code.</returns>
        /// <response code="200">Notifications were successfully updated.</response>
        /// <response code="400">NotificationType was wrong.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [HttpPut("{notificationType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ReadUsersNotificationsByType(NotificationType notificationType)
        {
            var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
            await notificationService.ReadUsersNotificationsByType(userId, notificationType).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Get Notification by it's id.
        /// </summary>
        /// <param name="id">Notification id.</param>
        /// <returns>Notification.</returns>
        /// <response code="200">All user's nofiticaitions.</response>
        /// <response code="400">Id was wrong.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await notificationService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Get new user's notification from the database.
        /// </summary>
        /// <returns>List of all grouped and single user's notifications.</returns>
        /// <response code="200">All user's nofiticaitions.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAllUsersNotificationsGroupedAsync()
        {
            var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var allNofitications = await notificationService.GetAllUsersNotificationsGroupedAsync(userId).ConfigureAwait(false);

            return Ok(allNofitications);
        }

        /// <summary>
        /// Get new user's notification from the database.
        /// </summary>
        /// <param name="notificationType">Type of notifications.</param>
        /// <returns>List of all user's notifications.</returns>
        /// <response code="200">All user's nofiticaitions.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAllUsersNotifications(NotificationType? notificationType)
        {
            var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var allNofitications = await notificationService.GetAllUsersNotificationsByFilterAsync(userId, notificationType).ConfigureAwait(false);

            return Ok(allNofitications);
        }

        /// <summary>
        /// Get amount of new notifications for user.
        /// </summary>
        /// <returns>Amount of all user's notifications.</returns>
        /// <response code="200">Amount of all user's nofiticaitions.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAmountOfNewUsersNotifications()
        {
            var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var amount = await notificationService.GetAmountOfNewUsersNotificationsAsync(userId).ConfigureAwait(false);

            return Ok(new { Amount = amount });
        }
    }
}
