using System;
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
        public async Task<ActionResult<IEnumerable<Workshop>>> GetWorkshops()
        {
            try
            {
                return Ok(await Task.FromResult(workshopService.GetAllWorkshops()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Add new workshop to the database.
        /// </summary>
        /// <param name="workshopDto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<ActionResult<Workshop>> CreateWorkshop(WorkshopDTO workshopDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var workshop = await workshopService.Create(workshopDto).ConfigureAwait(false);
                return CreatedAtAction(
                    nameof(GetWorkshops),
                    workshop);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}