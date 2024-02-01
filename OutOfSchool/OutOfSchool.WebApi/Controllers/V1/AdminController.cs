using Google.Apis.Discovery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class AdminController : Controller
{
    private readonly IStringLocalizer<SharedResource> localizer;

    private readonly ILogger<AdminController> logger;

    private readonly ICurrentUserService currentUserService;
    private readonly ISensitiveMinistryAdminService ministryAdminService;
    private readonly ISensitiveDirectionService directionService;
    private readonly ISensitiveProviderService providerService;
    private readonly ISensitiveApplicationService applicationService;


    public AdminController(
       ILogger<AdminController> logger,
       ICurrentUserService currentUserService,
       ISensitiveMinistryAdminService ministryAdminService,
       ISensitiveApplicationService applicationService,
       ISensitiveDirectionService directionService,
       ISensitiveProviderService providerService,
       IStringLocalizer<SharedResource> localizer)
    {
        this.localizer = localizer;
        this.logger = logger;
        this.applicationService = applicationService;
        this.directionService = directionService;
        this.providerService = providerService;
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
    }

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
         if (!User.IsInRole(nameof(Role.TechAdmin).ToLower()))
         {
             logger.LogError("You have no rights because you are not an admin");
             return StatusCode(403, "Forbidden to update another user if you don't have TechAdmin role.");
         }

         var ministryAdmins = await ministryAdminService.GetByFilter(filter).ConfigureAwait(false);

         if (ministryAdmins.TotalAmount < 1)
         {
             return NoContent();
         }

         return Ok(ministryAdmins);
     }

     /// <summary>
     /// Get all Providers from the database.
     /// </summary>
     /// <param name="filter">Filter to get a part of all providers that were found.</param>
     /// <returns>The result is a <see cref="SearchResult{ProviderDto}"/> that contains the count of all found providers and a list of providers that were received.</returns>
    [HasPermission(Permissions.ProviderRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilterProvider([FromQuery] ProviderFilter filter)
     {
         if (!User.IsInRole(nameof(Role.TechAdmin).ToLower()))
         {
             logger.LogError("You have no rights because you are not an admin");
             return StatusCode(403, "Forbidden to update another user if you don't have TechAdmin role.");
         }

         var providers = await providerService.GetByFilter(filter).ConfigureAwait(false);

         if (providers.TotalAmount < 1)
         {
             return NoContent();
         }

         return Ok(providers);
     }

     /// <summary>
    /// Get all applications from the database.
    /// </summary>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of all applications.</returns>
    /// <response code="200">All entities were found.</response>
    /// <response code="204">No entity was found.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.ApplicationRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetApplications([FromQuery] ApplicationFilter filter)
    {
        if (!User.IsInRole(nameof(Role.TechAdmin).ToLower()))
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to update another user if you don't have TechAdmin role.");
        }

        var applications = await applicationService.GetAll(filter).ConfigureAwait(false);

        if (!applications.Entities.Any())
        {
            return NoContent();
        }

        return Ok(applications);
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
        if (!User.IsInRole(nameof(Role.TechAdmin).ToLower()))
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to update another user if you don't have TechAdmin role.");
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
        if (!User.IsInRole(nameof(Role.TechAdmin).ToLower()))
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to update another user if you don't have TechAdmin role.");
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
    [HasPermission(Permissions.ProviderRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProviderByFilter([FromQuery] ProviderFilter filter)
    {
        if (!User.IsInRole(nameof(Role.TechAdmin).ToLower()))
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to update another user if you don't have TechAdmin role.");
        }

        var providers = await providerService.GetByFilter(filter).ConfigureAwait(false);

        return Ok(providers);
    }

    /// <summary>
    /// Block/unblock Provider.
    /// </summary>
    /// <param name="providerBlockDto">Entity to update.</param>
    /// <returns>Block Provider.</returns>
    [HasPermission(Permissions.ProviderBlock)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderBlockDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<ActionResult> BlockProvider([FromBody] ProviderBlockDto providerBlockDto)
    {
        if (!User.IsInRole(nameof(Role.TechAdmin).ToLower()))
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to update another user if you don't have TechAdmin role.");
        }

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
}