using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for a Child entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChildController : ControllerBase
    {
        private readonly IChildService childService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildController"/> class.
        /// </summary>
        /// <param name="childService">Service for Child model.</param>
        public ChildController(IChildService childService)
        {
            this.childService = childService;
        }

        /// <summary>
        /// Get all children from the database.
        /// </summary>
        /// <returns>List of all children.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult> GetChildren()
        {
            return Ok(await childService.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// Get child by it's id.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>Child entity.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChildById(long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(id.ToString(), $"The id cannot be less than 1.");
            }

            return Ok(await childService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Method for creating a new child.
        /// </summary>
        /// <param name="dto">Child entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateChild(ChildDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await childService.Create(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Update info about a specific child in the database.
        /// </summary>
        /// <param name="dto">Child entity.</param>
        /// <returns>Child's key.</returns>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<ActionResult> UpdateChild(ChildDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await childService.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Child entity from the database.
        /// </summary>
        /// <param name="id">Child's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteChild(long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(id.ToString(),
                    "The id cannot be less than 1.");
            }

            await childService.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}