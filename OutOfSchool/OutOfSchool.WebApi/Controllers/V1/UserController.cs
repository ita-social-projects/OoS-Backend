using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">Service for User model.</param>
        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Get all users from the database.
        /// </summary>
        /// <returns>List of all users.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ShortUserDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await userService.GetAll().ConfigureAwait(false);

            if (!users.Any())
            {
                return NoContent();
            }

            return Ok(users);
        }

        /// <summary>
        /// Get user by it's key.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>User element with some id.</returns>
        [HasPermission(Permissions.UserRead)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(string id)
        {
            string userId = User.FindFirst("sub")?.Value;

            if (userId != id)
            {
                return StatusCode(403, "Forbidden to get another user.");
            }

            var user = await userService.GetById(id).ConfigureAwait(false);

            if (user is null)
            {
                return NoContent();
            }

            return Ok(user);
        }

        /// <summary>
        /// Update info about the User.
        /// </summary>
        /// <param name="dto">Entity to update.</param>
        /// <returns>Updated Provider.</returns>
        [HasPermission(Permissions.UserEdit)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(ShortUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string userId = User.FindFirst("sub")?.Value;

            if (userId != dto.Id)
            {
                return StatusCode(403, "Forbidden to update another user.");
            }

            return Ok(await userService.Update(dto).ConfigureAwait(false));
        }
    }
}
