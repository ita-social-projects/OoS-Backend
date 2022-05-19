using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class SupportInformationController : CompanyInformationController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupportInformationController"/> class.
        /// </summary>
        /// <param name="companyInformationService">Service for SupportInformation model.</param>
        /// <param name="localizer">Localizer.</param>
        public SupportInformationController(
            ICompanyInformationService companyInformationService,
            IStringLocalizer<SharedResource> localizer)
            : base(companyInformationService, localizer)
        {
        }

        protected override CompanyInformationType Type { get => CompanyInformationType.SupportInformation; }
    }
}
