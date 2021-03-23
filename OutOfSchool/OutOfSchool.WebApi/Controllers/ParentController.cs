using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

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
        private readonly IParentService service;
        private readonly ILogger<ParentController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentController"/> class.
        /// Initialization of ParentController.
        /// </summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="service">Service for ParentCOntroller.</param>
        public ParentController(ILogger<ParentController> logger, IParentService service)
        {
            this.logger = logger;
            this.service = service;
        }

        /// <summary>
        /// To get all Parents from DB.
        /// </summary>
        /// <returns>List of Parents.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ParentDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
                return Ok(await service.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// To recieve parent with define id.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>Parent with define id.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                ParentDTO parentDTO = await service.GetById(id).ConfigureAwait(false);
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
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(ParentDTO parentDTO)
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
                parentDTO.Id = default;
                ParentDTO parent = await service.Create(parentDTO).ConfigureAwait(false);
                return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = parent.Id,
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                return Ok(await service.Update(parentDTO).ConfigureAwait(false));
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(long id)
        {
            try
            {
                await service.Delete(id).ConfigureAwait(false);
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ParentDTO>> GetProfile()
        {
            try
            {
                int id = int.Parse(this.GetJwtClaimByName("sid"));
                ParentDTO parentDTO = await service.GetById(id).ConfigureAwait(false);
                return this.Ok(parentDTO);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
    }
}
