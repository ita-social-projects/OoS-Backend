using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for subsubcategory entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SubsubcategoryController : ControllerBase
    {
        private readonly ISubsubcategoryService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsubcategoryController"/> class.
        /// Initialization of SubsubcategoryController.
        /// </summary>
        /// <param name="service">Service for SubsubcategoryCOntroller.</param>
        /// <param name="localizer">Localizer.</param>
        public SubsubcategoryController(ISubsubcategoryService service, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.service = service;
        }

        /// <summary>
        /// To get all Subsubcategories from DB.
        /// </summary>
        /// <returns>List of Subsubcategories.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SubsubcategoryDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var categories = await service.GetAll().ConfigureAwait(false);

            if (!categories.Any())
            {
                return NoContent();
            }

            return Ok(categories);
        }

        /// <summary>
        /// To recieve Subsubcategory with define id.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>Subsubcategory with define id.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubsubcategoryDTO))]
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
        /// To get all Subsubcategories from DB with some SubcategoryId.
        /// <param name="id">The subcategory Id.</param>
        /// </summary>
        /// <returns>List of Subsubcategories.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SubsubcategoryDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBySubcategoryId(long id)
        {
            try
            {
                var categories = await service.GetBySubcategoryId(id).ConfigureAwait(false);

                if (!categories.Any())
                {
                    return NoContent();
                }

                return Ok(categories);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To create new Subsubcategory and add to the DB.
        /// </summary>
        /// <param name="dto">SubsubcategoryDTO object that we want to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "admin")]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(SubsubcategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                dto.Id = default;

                var category = await service.Create(dto).ConfigureAwait(false);

                return CreatedAtAction(
                     nameof(GetById),
                     new { id = category.Id, },
                     category);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To update Subsubcategory entity that already exists.
        /// </summary>
        /// <param name="categoryDTO">SubsubcategoryDTO object with new properties.</param>
        /// <returns>Subsubcategory's key.</returns>
        [Authorize(Roles = "admin")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubsubcategoryDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(SubsubcategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(categoryDTO).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete Subsubcategory entity from DB.
        /// </summary>
        /// <param name="id">The key in table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "admin")]
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
    }
}
