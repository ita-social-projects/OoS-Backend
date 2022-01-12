using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class SupportInformationController : ControllerBase
    {
        private readonly ISupportInformationService supportInformationService;
        private readonly ILogger<SupportInformationController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportInformationController"/> class.
        /// </summary>
        /// <param name="supportInformationService">Service for SupportInformation model.</param>
        /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
        public SupportInformationController(
            ISupportInformationService supportInformationService,
            ILogger<SupportInformationController> logger)
        {
            this.supportInformationService = supportInformationService;
            this.logger = logger;
        }

        /// <summary>
        /// Get support information from the database.
        /// </summary>
        /// <returns>Support information.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SupportInformationDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var supportInformation = await supportInformationService.GetSupportInformation().ConfigureAwait(false);

            if (supportInformation is null)
            {
                return NoContent();
            }

            return Ok(supportInformation);
        }

        /// <summary>
        /// Update support information.
        /// </summary>
        /// <param name="supportInformationModel">Entity to update.</param>
        /// <returns>Updated support information.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SupportInformationDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(SupportInformationDto supportInformationModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var supportInformation = await supportInformationService.Update(supportInformationModel).ConfigureAwait(false);

            if (supportInformation is null)
            {
                return BadRequest($"Cannot change support.{Environment.NewLine}Please check information is valid.");
            }

            return Ok(supportInformation);
        }
    }
}
