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
    /// Controller with CRUD operations for Section entity.
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
        /// <param name="sectionService">Service for Section model.</param>
        public SectionController(ISectionService sectionService)
        {
            this.sectionService = sectionService;
        }

        /// <summary>
        /// Get all sections from the database.
        /// </summary>
        /// <returns>List of all sections.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Section>> GetSections()
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
        /// <param name="sectionDto">Entity which needs to be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<ActionResult<Section>> CreateSection(SectionDTO sectionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var section = await sectionService.Create(sectionDto).ConfigureAwait(false);
                return CreatedAtAction(
                    nameof(GetSections),
                    new
                    {
                        id = section.Id,
                    },
                    section);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}