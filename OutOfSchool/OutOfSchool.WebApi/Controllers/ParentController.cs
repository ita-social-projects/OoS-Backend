using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for parent entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
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
        public async Task<ActionResult<IEnumerable<ParentDTO>>> GetParents()
        {
            try
            {
                var parents = await parentService.GetAll().ConfigureAwait(false);
                return Ok(parents);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                ParentDTO parentDTO = await parentService.GetById(id).ConfigureAwait(false);
                return Ok(parentDTO);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                ParentDTO parent = await parentService.Create(parentDTO).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetParentById), new
                {
                  id = parent.Id 
                },
                parent);
            }

            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                return BadRequest("Entity was null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                return Ok(await parentService.Update(parentDTO).ConfigureAwait(false));
            }

            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                await parentService.Delete(id).ConfigureAwait(false);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
