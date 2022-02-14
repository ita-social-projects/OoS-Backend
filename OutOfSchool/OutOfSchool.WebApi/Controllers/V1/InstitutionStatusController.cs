using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class InstitutionStatusController : ControllerBase
    {
        private readonly IStatusService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionStatusController"/> class.
        /// </summary>
        /// <param name="service">Service for InstitutionStatus model.</param>
        /// <param name="localizer">Localizer.</param>
        public InstitutionStatusController(IStatusService service, IStringLocalizer<SharedResource> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get all Institution Statuses from the database.
        /// </summary>
        /// <returns>List of all Institution Statuses.</returns>
        [HasPermission(Permissions.ImpersonalDataRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InstitutionStatusDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var institutionStatuses = await service.GetAll().ConfigureAwait(false);

            if (!institutionStatuses.Any())
            {
                return NoContent();
            }

            return Ok(institutionStatuses);
        }

        /// <summary>
        /// Get Institution Status by it's id.
        /// </summary>
        /// <param name="id">Institution Status id.</param>
        /// <returns>Institution Status.</returns>
        [HasPermission(Permissions.ImpersonalDataRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InstitutionStatusDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                this.ValidateId(id, localizer);
                return Ok(await service.GetById(id).ConfigureAwait(false));
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }

        }

        /// <summary>
        /// Add a new Institution Status to the database.
        /// </summary>
        /// <param name="dto">Institution Status entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(InstitutionStatusDTO dto)
        {
            var institutionStatus = await service.Create(dto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = institutionStatus.Id, },
                institutionStatus);
        }

        /// <summary>
        /// Update info about a Institution Status in the database.
        /// </summary>
        /// <param name="dto">Institution Status to update.</param>
        /// <returns>Institution Status.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InstitutionStatusDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(InstitutionStatusDTO dto)
        {
            return Ok(await service.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Institution Status from the database.
        /// </summary>
        /// <param name="id">Institution Status id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                this.ValidateId(id, localizer);
                await service.Delete(id).ConfigureAwait(false);
                return NoContent();
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }


        }

    }
}
