using System.Net.Mime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class AdminController : Controller
{
    private readonly IStringLocalizer<SharedResource> localizer;

    private readonly ILogger<AdminController> logger;

    private readonly ISensitiveMinistryAdminService ministryAdminService;
    private readonly ISensitiveDirectionService directionService;
    private readonly ISensitiveProviderService providerService;
    private readonly ISensitiveWorkshopsService workshopService;
    private readonly ISensitiveApplicationService applicationService;
    private readonly ISensitiveWorkshopDraftService workshopDraftService;

    public AdminController(
        ILogger<AdminController> logger,
        ISensitiveMinistryAdminService ministryAdminService,
        ISensitiveApplicationService applicationService,
        ISensitiveDirectionService directionService,
        ISensitiveProviderService providerService,
        ISensitiveWorkshopsService workshopService,
        IStringLocalizer<SharedResource> localizer,
        ISensitiveWorkshopDraftService workshopDraftService)
    {
        this.localizer = localizer;
        this.logger = logger;
        this.applicationService = applicationService;
        this.directionService = directionService;
        this.providerService = providerService;
        this.workshopService = workshopService;
        this.ministryAdminService =
            ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.workshopDraftService =
            workshopDraftService ?? throw new ArgumentNullException(nameof(workshopDraftService));
    }

    private bool IsTechAdmin() => User.IsInRole(nameof(Role.TechAdmin).ToLower());

    /// <summary>
    /// Get MinistryAdmins that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{MinistryAdminDto}"/>, or no content.</returns>
    [HasPermission(Permissions.MinistryAdminRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<MinistryAdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilterMinistryAdmin([FromQuery] MinistryAdminFilter filter)
    {
        var ministryAdmins = await ministryAdminService.GetByFilter(filter).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(ministryAdmins);
    }

    /// <summary>
    /// Get all applications from the database.
    /// </summary>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of all applications.</returns>
    /// <response code="200">All entities were found.</response>
    /// <response code="204">No entity was found.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.AdminDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetApplications([FromQuery] ApplicationFilter filter)
    {
        var applications = await applicationService.GetAll(filter).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(applications);
    }

    /// <summary>
    /// To update Direction entity that already exists.
    /// </summary>
    /// <param name="directionDto">DirectionDto object with new properties.</param>
    /// <returns>Direction that was updated.</returns>
    /// <response code="200">Direction was successfully updated.</response>
    /// <response code="400">Model is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DirectionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateDirections(DirectionDto directionDto)
    {
        if (!IsTechAdmin())
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to update directions if you don't have TechAdmin role.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Ok(await directionService.Update(directionDto).ConfigureAwait(false));
    }

    /// <summary>
    /// Delete the Direction entity from DB.
    /// </summary>
    /// <param name="id">The key of the Direction in table.</param>
    /// <returns>Status Code.</returns>
    /// <response code="204">Direction was successfully deleted.</response>
    /// <response code="400">If some workshops assosiated with this direction.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [FeatureGate(nameof(Feature.ShowForProduction))]
    public async Task<ActionResult> DeleteDirectionById(long id)
    {
        if (!IsTechAdmin())
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to delete direction if you don't have TechAdmin role.");
        }

        this.ValidateId(id, localizer);

        var result = await directionService.Delete(id).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return BadRequest(result.OperationResult);
        }

        return NoContent();
    }

    /// <summary>
    /// Get all Providers from the database.
    /// </summary>
    /// <param name="filter">Filter to get a part of all providers that were found.</param>
    /// <returns>The result is a <see cref="SearchResult{ProviderDto}"/> that contains the count of all found providers and a list of providers that were received.</returns>
    [HasPermission(Permissions.AdminDataRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProviderByFilter([FromQuery] ProviderFilter filter)
    {
        var providers = await providerService.GetByFilter(filter).ConfigureAwait(false);

        //TODO clarify frontend about if statement
        return this.SearchResultToOkOrNoContent(providers);
    }

    /// <summary>
    /// Block/unblock Provider.
    /// </summary>
    /// <param name="providerBlockDto">Entity to update.</param>
    /// <returns>Block Provider.</returns>
    [HasPermission(Permissions.ProviderBlock)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderBlockDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<ActionResult> BlockProvider([FromBody] ProviderBlockDto providerBlockDto)
    {
        var result = await providerService.Block(
            providerBlockDto,
            await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false));

        if (!result.IsSuccess)
        {
            switch (result.HttpStatusCode)
            {
                case HttpStatusCode.Forbidden:
                    return Forbid();
                case HttpStatusCode.NotFound:
                    return NotFound(result.Message);
                default:
                    return NotFound(result.Message);
            }
        }

        return Ok(result.Result);
    }

    /// <summary>
    /// Get all Workshops from the database by filter.
    /// </summary>
    /// <param name="filter">Filter to get a part of all workshops that were found.</param>
    /// <returns>The result is a <see cref="SearchResult{WorkshopDto}"/> that contains the count of all found workshops and list of workshops that were received.</returns>
    [HasPermission(Permissions.WorkshopApprove)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetWorkshopsByFilter([FromQuery] WorkshopFilterAdministration filter)
    {
        var workshops = await workshopService.FetchByFilterForAdmins(filter).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(workshops);
    }

    /// <summary>
    /// Check providers for existing entities by data from incoming parameter.
    /// </summary>
    /// <param name="data">Values for checking.</param>
    /// <returns>Crossing data.</returns>
    [Authorize(Roles = "techadmin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImportDataValidateResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("~/api/v{version:apiVersion}/providers/import/validate")]
    [FeatureGate(nameof(Feature.TechAdminImport))]
    public async Task<ActionResult> ValidateImportData([FromBody] ImportDataValidateRequest data)
    {
        var result = await providerService.ValidateImportData(data).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Export all Providers to CSV file.
    /// </summary>
    /// <returns>CSV file containing all providers.</returns>
    [HttpGet("~/api/v{version:apiVersion}/admin/providers/export")]
    [Authorize(Roles = "techadmin")]
    [FeatureGate(nameof(Feature.TechAdminExport))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportProviders()
    {
        var providersCsvData = await providerService.GetCsvExportData().ConfigureAwait(false);

        if (providersCsvData is null or { Length: 0 })
        {
            return NoContent();
        }

        return File(providersCsvData, MediaTypeNames.Text.Csv, "providers.csv");
    }

    /// <summary>
    /// Get all Workshop Drafts from the database by filter.
    /// </summary>
    /// <param name="filter">Filter to get a part of all workshops that were found.</param>
    /// <returns>The result is a <see cref="SearchResult{WorkshopV2Dto}"/> that contains the count of all found workshops and list of workshops that were received.</returns>
    [HasPermission(Permissions.WorkshopApprove)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopV2Dto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetWorkshopDraftsByFilter([FromQuery] WorkshopDraftFilterAdministration filter) =>    
         await workshopDraftService.FetchByFilterForAdmins(filter).ProtectAndMap(this.SearchResultToOkOrNoContent);     
}