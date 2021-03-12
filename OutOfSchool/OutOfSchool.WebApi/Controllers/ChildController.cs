using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IChildService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildController"/> class.
        /// </summary>
        /// <param name="service">Service for Child model.</param>
        public ChildController(IChildService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Get all children from the database.
        /// </summary>
        /// <returns>List of all children.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await service.GetAll().ConfigureAwait(false));
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
        public async Task<IActionResult> GetById(long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(id.ToString(), $"The id cannot be less than 1.");
            }

            return Ok(await service.GetById(id).ConfigureAwait(false));
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
        public async Task<IActionResult> Create(ChildDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Create(dto).ConfigureAwait(false));
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
        public async Task<ActionResult> Update(ChildDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(dto).ConfigureAwait(false));
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
        public async Task<ActionResult> Delete(long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(id.ToString(),
                    "The id cannot be less than 1.");
            }

            await service.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}