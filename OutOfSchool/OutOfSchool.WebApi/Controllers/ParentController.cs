using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentController"/> class.
        /// Initialization of ParentController.
        /// </summary>
        /// <param name="service">Service for ParentCOntroller.</param>
        /// <param name="localizer">Localizer.</param>
        public ParentController(IParentService service, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
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
            var parents = await service.GetAll().ConfigureAwait(false);

            if (!parents.Any())
            {
                return NoContent();
            }

            return Ok(parents);
        }

        /// <summary>
        /// To recieve parent with define id.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>Parent with define id.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            return Ok(await service.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// To create new Parent and add to the DB.
        /// </summary>
        /// <param name="dto">ParentDTO object that we want to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(ParentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                dto.Id = default;
                dto.UserId = User.FindFirst("sub")?.Value;

                var parent = await service.Create(dto).ConfigureAwait(false);

                return CreatedAtAction(
                     nameof(GetById),
                     new { id = parent.Id, },
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(parentDTO).ConfigureAwait(false));
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
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            await service.Delete(id).ConfigureAwait(false);

            return NoContent();
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
