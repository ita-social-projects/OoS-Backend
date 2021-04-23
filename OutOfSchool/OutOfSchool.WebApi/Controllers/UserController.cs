using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">Service for User model.</param>
        /// <param name="localizer">Localizer.</param>
        public UserController(IUserService userService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.userService = userService;
        }

        /// <summary>
        /// Get all users from the database.
        /// </summary>
        /// <returns>List of all users.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await userService.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// Get user by it's key.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>User element with some id.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                return Ok(await userService.GetById(id).ConfigureAwait(false));
            }
            catch (ArgumentException)
            {
                throw;
            }                  
        }
    }
}
