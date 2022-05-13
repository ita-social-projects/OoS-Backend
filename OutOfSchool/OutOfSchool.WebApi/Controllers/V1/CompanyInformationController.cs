using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Base Controller with CRUD operations for CompanyInformation entity.
    /// </summary>
    public abstract class CompanyInformationController : ControllerBase
    {
        private readonly ICompanyInformationService companyInformationService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<AboutPortalController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyInformationController"/> class.
        /// </summary>
        /// <param name="companyInformationService">Service for CompanyInformation model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
        public CompanyInformationController(
            ICompanyInformationService companyInformationService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<AboutPortalController> logger)
        {
            this.companyInformationService = companyInformationService ?? throw new ArgumentNullException(nameof(companyInformationService));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets company information type property.
        /// </summary>
        protected abstract CompanyInformationType Type { get; }

        /// <summary>
        /// Get information about CompanyInformation from the database by Type.
        /// </summary>
        /// <returns>Information about CompanyInformation by Type.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AboutPortalDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var aboutPortal = await this.companyInformationService.GetByType(this.Type).ConfigureAwait(false);

            if (aboutPortal == null)
            {
                return this.NoContent();
            }

            return this.Ok(aboutPortal);
        }

        /// <summary>
        /// Update information by Type about CompanyInformation.
        /// </summary>
        /// <param name="companyInformationModel">Entity to update.</param>
        /// <returns>Updated information about CompanyInformation.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AboutPortalDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(AboutPortalDto companyInformationModel)
        {
            companyInformationModel.Type = this.Type;

            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var aboutPortal = await this.companyInformationService.Update(companyInformationModel).ConfigureAwait(false);

            if (aboutPortal == null)
            {
                return this.BadRequest(this.localizer["Cannot change information about {0}.\n Please check information is valid.", this.Type]);
            }

            return this.Ok(aboutPortal);
        }
    }
}
