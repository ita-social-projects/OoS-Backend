using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
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
    private readonly ISensitiveWorkshopDraftService workshopDraftService;
    private readonly IUserService userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class with required services for administrative operations.
    /// </summary>
    /// <param name="logger">Logger for recording controller operations.</param>
    /// <param name="ministryAdminService">Service for managing ministry admin entities.</param>
    /// <param name="directionService">Service for managing direction entities.</param>
    /// <param name="providerService">Service for managing provider entities.</param>
    /// <param name="workshopService">Service for managing workshop entities.</param>
    /// <param name="localizer">Localization service for shared resources.</param>
    /// <param name="workshopDraftService">Service for managing workshop draft entities.</param>
    /// <param name="userService">Service for managing user profiles and account status.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required service is null.</exception>
    public AdminController(
        ILogger<AdminController> logger,
        ISensitiveMinistryAdminService ministryAdminService,
        ISensitiveDirectionService directionService,
        ISensitiveProviderService providerService,
        ISensitiveWorkshopsService workshopService,
        IStringLocalizer<SharedResource> localizer,
        ISensitiveWorkshopDraftService workshopDraftService,
        IUserService userService)
    {
        this.localizer = localizer;
        this.logger = logger;
        this.directionService = directionService;
        this.providerService = providerService;
        this.workshopService = workshopService;
        this.ministryAdminService =
            ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.workshopDraftService =
            workshopDraftService ?? throw new ArgumentNullException(nameof(workshopDraftService));
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <summary>
/// Determines whether the current user has the "techadmin" role.
/// </summary>
/// <returns>True if the user is a technical administrator; otherwise, false.</returns>
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
    [FeatureGate(nameof(Feature.DirectionManagement))]
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
    [FeatureGate(nameof(Feature.DirectionManagement))]
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
    /// <summary>
    /// Validates provider import data and returns the validation result.
    /// </summary>
    /// <param name="data">The import data to validate.</param>
    /// <returns>The validation result for the provided import data.</returns>
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
    /// Get all Workshop Drafts from the database by filter.
    /// </summary>
    /// <param name="filter">Filter to get a part of all workshops that were found.</param>
    /// <summary>
         /// Retrieves workshop drafts matching the specified administrative filter.
         /// </summary>
         /// <param name="filter">Criteria for filtering workshop drafts.</param>
         /// <returns>A <see cref="SearchResult{WorkshopDraftResponseDto}"/> containing the total count and list of matching workshop drafts, or 204 No Content if none are found.</returns>
    [HasPermission(Permissions.WorkshopApprove)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopDraftResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetWorkshopDraftsByFilter([FromQuery] WorkshopDraftFilterAdministration filter) =>
         await workshopDraftService.FetchByFilterForAdmins(filter).ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// To Get the Profile of authorized Technical Staff (Techadmin and Moderator).
    /// </summary>
    /// <summary>
    /// Retrieves the profile information and account status of the currently authorized technical staff member.
    /// </summary>
    /// <returns>The technical staff profile as a <see cref="TechnicalStaffDto"/> if found; returns 400 Bad Request if user information is invalid, or 404 Not Found if the user does not exist.</returns>
    [Authorize(Roles = "techadmin, moderator")]
    [HasPermission(Permissions.PersonalInfo)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TechnicalStaffDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = GettingUserProperties.GetUserId(User);

        if (userId == null)
        {
            return BadRequest("Invalid user information.");
        }

        try
        {
            BaseUserDto user = await userService.GetById(userId).ConfigureAwait(false);
            var technicalStaff = user.ToTechnicalStaffDto();
            technicalStaff.AccountStatus = await userService.GetAccountStatus(userId).ConfigureAwait(false);
            return Ok(technicalStaff);
        }
        catch (ArgumentException e)
        {
            return NotFound(e.Message);
        }
    }
}