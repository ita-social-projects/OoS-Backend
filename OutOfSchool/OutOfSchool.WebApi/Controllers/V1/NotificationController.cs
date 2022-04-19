using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using GrpcService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Notifications;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="notificationService">Service for Notification model.</param>
        public NotificationController(
            INotificationService notificationService)
        {
            this.notificationService = notificationService;
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
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationDto))]
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
            var userId = GettingUserProperties.GetUserId(User);
            await notificationService.ReadUsersNotificationsByType(userId, notificationType).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Get Notification by it's id.
        /// </summary>
        /// <param name="id">Notification id.</param>
        /// <returns>Notification.</returns>
        /// <response code="200">Returns Nofiticaition.</response>
        /// <response code="400">Id was wrong.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var token = await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    metadata.Add("Authorization", $"Bearer {token}");
                }
                return Task.CompletedTask;
            });

            var defaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { Grpc.Core.StatusCode.Unavailable },
                },
            };

            // SslCredentials is used here because this channel is using TLS.
            // CallCredentials can't be used with ChannelCredentials.Insecure on non-TLS channels.
            var channel = GrpcChannel.ForAddress("https://localhost:5002", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
                ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            });

            var client = new gRPC_ProviderAdmin.gRPC_ProviderAdminClient(channel);

            var _request = new CreateRequest()
            {
                FirstName = "FirstName",
                LastName = "LastName",
                MiddleName = "MiddleName",
                Email = "romanchuk.o@slm.ua",
                PhoneNumber = "+380681111111",
                CreatingTime = Timestamp.FromDateTimeOffset(DateTimeOffset.Now),
                ReturnUrl = "ReturnUrl",
                ProviderId = "12300851-66db-4236-9271-1f037ffe3101",
                UserId = "cee4ec9a-6fe1-444d-9d33-9df6f3d0f3ee",
                IsDeputy = true,
                ManagedWorkshopIds = { new List<string>() { "12316600-66db-4236-9271-1f037ffe3101", "12316600-66db-4236-9271-1f037ffe3102" } },
            };

            var reply = new CreateReply();

            try
            {
                reply = await client.CreateProviderAdminAsync(_request);
            }
            catch (RpcException ex)
            {
                var i = ex;
            }

            return Ok(reply);
        }

        /// <summary>
        /// Get new user's notification from the database.
        /// </summary>
        /// <returns>List of all grouped and single user's notifications.</returns>
        /// <response code="200">All user's nofiticaitions.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationGroupedAndSingle))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersNotificationsGroupedAsync()
        {
            var userId = GettingUserProperties.GetUserId(User);

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
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<NotificationDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersNotifications(NotificationType? notificationType)
        {
            var userId = GettingUserProperties.GetUserId(User);

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
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationAmount))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAmountOfNewUsersNotifications()
        {
            var userId = GettingUserProperties.GetUserId(User);

            var amount = await notificationService.GetAmountOfNewUsersNotificationsAsync(userId).ConfigureAwait(false);

            return Ok(new NotificationAmount() { Amount = amount });
        }
    }
}
