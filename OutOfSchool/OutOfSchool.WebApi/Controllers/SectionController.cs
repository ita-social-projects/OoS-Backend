using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for Workshop entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService sectionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionController"/> class.
        /// </summary>
        /// <param name="sectionService">Service for Workshop model.</param>
        public SectionController(ISectionService sectionService)
        {
            this.sectionService = sectionService;
        }

        /// <summary>
        /// Get all sections from the database.
        /// </summary>
        /// <returns>List of all sections.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Workshop>> GetSections()
        {
            try
            {
                return Ok(sectionService.GetAllSections());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Add new section to the database.
        /// </summary>
        /// <param name="workshopDto">Entity which needs to be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<ActionResult<Workshop>> CreateSection(WorkshopDTO workshopDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var section = await sectionService.Create(workshopDto).ConfigureAwait(false);
                return CreatedAtAction(
                    nameof(GetSections),
                    section);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}