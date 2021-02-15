using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for a Child entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class ChildrenController : ControllerBase
    {
        private IChildService childService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildrenController"/> class.
        /// </summary>
        /// <param name="childService">Service for Child model.</param>
        public ChildrenController(IChildService childService)
        {
            this.childService = childService;
        }

        /// <summary>
        /// Get all children from database.
        /// </summary>
        /// <returns>List of all children.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Child>> GetChildren()
        {
            try
            {
                return this.Ok(this.childService.GetAll());
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get child by id.
        /// </summary>
        /// <param name="id">Key in database.</param>
        /// <returns>Child element with some id.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ChildDTO>> GetChildById(long id)
        {
            try
            {
                ChildDTO childDTO = await this.childService.GetById(id).ConfigureAwait(false);
                return this.Ok(childDTO);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Method for create new child.
        /// </summary>
        /// <param name="childDTO">Element which must be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPost]
        public async Task<ActionResult<Child>> CreateChild(ChildDTO childDTO)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                ChildDTO child = await this.childService.Create(childDTO).ConfigureAwait(false);
                return this.CreatedAtAction(
                    nameof(this.GetChildren),
                    new { id = child.Id },
                    child);
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update info about some child in database.
        /// </summary>
        /// <param name="childDTO">Entity.</param>
        /// <returns>Child's key.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPut]
        public async Task<ActionResult> Update(ChildDTO childDTO)
        {
            if (childDTO == null)
            {
                return this.BadRequest("Entity was null.");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                this.childService.Update(childDTO);
                return this.Ok(await this.childService.GetById(childDTO.Id).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete some element from database.
        /// </summary>
        /// <param name="id">Element's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            try
            {
                await this.childService.Delete(id).ConfigureAwait(false);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
    }
}
