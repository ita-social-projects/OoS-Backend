using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller with CRUD operations for LawsAndRegulations entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class LawsAndRegulationsController : CompanyInformationController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LawsAndRegulationsController"/> class.
        /// </summary>
        /// <param name="companyInformationService">Service for LawsAndRegulations model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
        public LawsAndRegulationsController(
            ICompanyInformationService companyInformationService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<AboutPortalController> logger)
            : base(companyInformationService, localizer, logger)
        {
        }

        /// <summary>
        /// Gets Type LawsAndRegulations for CompanyInformation.
        /// </summary>
        protected override CompanyInformationType Type { get => CompanyInformationType.LawsAndRegulations; }
    }
}