using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for Workshop entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class WorkshopController : ControllerBase
    {
        private readonly IWorkshopService workshopService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopController"/> class.
        /// </summary>
        /// <param name="workshopService">Service for Workshop model.</param>
        public WorkshopController(IWorkshopService workshopService)
        {
            this.workshopService = workshopService;
        }

        /// <summary>
        /// Get all workshops from the database.
        /// </summary>
        /// <returns>List of all workshops.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetWorkshops()
        {
            return Ok(await workshopService.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// Get workshop by it's id.
        /// </summary>
        /// <param name="id">Key in the database.</param>
        /// <returns>Workshop entity.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkshopById(long id)
        {
            if (id < 1 || workshopService.GetAll().Result.AsQueryable().Count() < id)
            {
                throw new ArgumentOutOfRangeException(id.ToString(), "The id is less than 1 or greater than number of table entities.");
            }
            
            return Ok(workshopService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Add new workshop to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateWorkshop(WorkshopDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await workshopService.Create(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Update info about workshop entity.
        /// </summary>
        /// <param name="dto">Workshop to update.</param>
        /// <returns>Workshop.</returns>
        [Authorize(Roles = "organization,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(WorkshopDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            return Ok( await workshopService.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific workshop entity from the database.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id < 1 || workshopService.GetAll().Result.AsQueryable().Count() < id)
            {
                throw new ArgumentOutOfRangeException(id.ToString(),
                    "The id is less than 1 or greater than number of table entities.");
            }
            
            await workshopService.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}