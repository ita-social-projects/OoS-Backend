using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models.BlockedProviderParent;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class BlockedProviderParentController : ControllerBase
    {
        private readonly IBlockedProviderParentService blockedProviderParentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockedProviderParentController"/> class.
        /// </summary>
        /// <param name="blockedProviderParentService">Service for BlockedProviderParent model.</param>
        public BlockedProviderParentController(
            IBlockedProviderParentService blockedProviderParentService)
        {
            this.blockedProviderParentService = blockedProviderParentService;
        }

        /// <summary>
        /// Add a new Notification to the database.
        /// </summary>
        /// <param name="blockedProviderParentBlockDto">blockedProviderParentBlock entity to add.</param>
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
        public async Task<IActionResult> Create(BlockedProviderParentBlockDto blockedProviderParentBlockDto)
        {
            var blockedProviderParentDto = await blockedProviderParentService.Block(blockedProviderParentBlockDto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = blockedProviderParentDto.Id, },
                blockedProviderParentDto);
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
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlockedProviderParentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok();
        }
    }
}
