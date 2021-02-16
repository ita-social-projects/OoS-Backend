using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD for Parent Table
    /// </summary>
    public class ParentController : ControllerBase
    {
        private readonly IParentService parentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentController"/> class.
        /// Initialization of ParentController.
        /// </summary>
        /// <param name="parentService">Service for ParentCOntroller</param>
        public ParentController(IParentService parentService)
        {
            this.parentService = parentService;
        }

        /// <summary>
        /// To get all Parents from DB.
        /// </summary>
        /// <returns>List of Parents.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ParentDTO>> GetParent()
        {
            try
            {
                return this.Ok(this.parentService.GetAll());
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To recieve parent with define id.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>Parent with define id.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ParentDTO>> GetParentById(long id)
        {
            try
            {
                ParentDTO parentDTO = await this.parentService.GetById(id).ConfigureAwait(false);
                return this.Ok(parentDTO);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To create new Parent and add to the DB.
        /// </summary>
        /// <param name="parentDTO">ParentDTO object that we want to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPost]
        public async Task<ActionResult<ParentDTO>> CreateParent(ParentDTO parentDTO)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                ParentDTO parent = await this.parentService.Create(parentDTO).ConfigureAwait(false);
                return this.CreatedAtAction(
                    nameof(this.GetParent),
                    new { id = parent.Id },
                    parent);
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To update Parent entity that already exists.
        /// </summary>
        /// <param name="parentDTO">ParentDTO object with new properties.</param>
        /// <returns>Parent's key.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPut]
        public async Task<ActionResult> Update(ParentDTO parentDTO)
        {
            if (parentDTO == null)
            {
                return this.BadRequest("Entity was null.");
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
               await this.parentService.Update(parentDTO).ConfigureAwait(false);
               return this.Ok(await this.parentService.GetById(parentDTO.Id).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete Parent entity from DB.
        /// </summary>
        /// <param name="id">The key in table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            try
            {
                await this.parentService.Delete(id).ConfigureAwait(false);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To Get the Profile of authorized Parent.
        /// </summary>
        /// <returns>Authorized parent's profile.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpGet]
        public async Task<ActionResult<ParentDTO>> GetParentProfile()
        {
            try
            {
                int id = int.Parse(this.GetJwtClaimByName("sid"));
                ParentDTO parentDTO = await this.parentService.GetById(id).ConfigureAwait(false);
                return this.Ok(parentDTO);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
    }
}
