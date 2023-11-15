using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

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
    public AboutPortalController(
        ICompanyInformationService companyInformationService,
        IStringLocalizer<SharedResource> localizer)
        : base(companyInformationService, localizer)
    {
    }

    protected override CompanyInformationType Type { get => CompanyInformationType.AboutPortal; }
}