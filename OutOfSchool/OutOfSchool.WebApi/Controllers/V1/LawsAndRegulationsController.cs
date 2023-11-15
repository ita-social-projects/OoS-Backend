using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

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
    public LawsAndRegulationsController(
        ICompanyInformationService companyInformationService,
        IStringLocalizer<SharedResource> localizer)
        : base(companyInformationService, localizer)
    {
    }

    /// <summary>
    /// Gets Type LawsAndRegulations for CompanyInformation.
    /// </summary>
    protected override CompanyInformationType Type { get => CompanyInformationType.LawsAndRegulations; }
}