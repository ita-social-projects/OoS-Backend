using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkshopDTO>>> GetWorkshops()
        {
            var workshops = await workshopService.GetAll().ConfigureAwait(false);

            return Ok(workshops);
        }

        /// <summary>
        /// Get workshop by it's id.
        /// </summary>
        /// <param name="id">Key in the database.</param>
        /// <returns>Workshop entity.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetWorkshopById(long id)
        {
            var workshop = await workshopService.GetById(id).ConfigureAwait(false);

            return Ok(workshop);
        }

        /// <summary>
        /// Add new workshop to the database.
        /// </summary>
        /// <param name="workshopDto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [HttpPost]
        public async Task<ActionResult> CreateWorkshop(WorkshopDTO workshopDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var workshop = await workshopService.Create(workshopDto).ConfigureAwait(false);
           
            return CreatedAtAction(nameof(GetWorkshopById), new
            {
                id = workshop.Id,
            });
        }

        /// <summary>
        /// Update info about workshop entity.
        /// </summary>
        /// <param name="workshopDto">Workshop to update.</param>
        /// <returns>Workshop.</returns>
        [Authorize(Roles = "organization,admin")]
        [HttpPut]
        public async Task<ActionResult> Update(WorkshopDTO workshopDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await workshopService.Update(workshopDto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific workshop entity from the database.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            if (id == 0)
            {
                return BadRequest("Id cannot be 0.");
            }

            await workshopService.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}