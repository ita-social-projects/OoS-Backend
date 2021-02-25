﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        private IChildService childService;

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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Child>>> GetChildren()
        {
            return Ok(await childService.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// Get child by it's id.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>Child entity.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ChildDTO>> GetChildById(long id)
        {
            if (id == 0)
            {
                return BadRequest("Id cannot be 0.");
            }
            
            var childDTO = await childService.GetById(id).ConfigureAwait(false);

            return Ok(childDTO);
        }

        /// <summary>
        /// Method for creating a new child.
        /// </summary>
        /// <param name="childDTO">Child entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPost]
        public async Task<ActionResult<Child>> CreateChild(ChildDTO childDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var child = await childService.Create(childDTO).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetChildById),
                new
                {
                    id = child.Id,
                });
        }

        /// <summary>
        /// Update info about a specific child in the database.
        /// </summary>
        /// <param name="childDTO">Child entity.</param>
        /// <returns>Child's key.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPut]
        public async Task<ActionResult> Update(ChildDTO childDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await childService.Update(childDTO).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Child entity from the database.
        /// </summary>
        /// <param name="id">Child's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            if (id == 0)
            {
                return BadRequest("Id cannot be 0.");
            }

            await childService.Delete(id).ConfigureAwait(false);
            
            return Ok();
        }
    }
}