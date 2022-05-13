﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller with CRUD operations for AboutPortal entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class AboutPortalController : CompanyInformationController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPortalController"/> class.
        /// </summary>
        /// <param name="companyInformationService">Service for AboutPortal model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
        public AboutPortalController(
            ICompanyInformationService companyInformationService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<AboutPortalController> logger)
            : base(companyInformationService, localizer, logger)
        {
        }

        protected override CompanyInformationType Type { get => CompanyInformationType.AboutPortal; }
    }
}