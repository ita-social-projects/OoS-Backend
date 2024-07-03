using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.Admin.Helpers.Authorization;
using OutOfSchool.Admin.MediatR.Applications.Queries;
using OutOfSchool.Admin.MediatR.Directions.Commands;
using OutOfSchool.Admin.MediatR.MinistryAdmin.Queries;
using OutOfSchool.Admin.MediatR.Providers.Commands;
using OutOfSchool.Admin.MediatR.Providers.Queries;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class AdminController(
    ISender sender,
    ILogger<AdminController> logger,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    private readonly ISender sender = sender;
    private readonly ILogger<AdminController> logger = logger;
    private readonly IStringLocalizer<SharedResource> localizer = localizer;

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
        var ministryAdmins = await sender.Send(new GetByFilterMinistryAdminsQuery(filter));

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
        var applications = await sender.Send(new GetByFilterAllApplicationsQuery(filter));

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
        if (!AuthorizationHelper.IsTechAdmin(User))
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to update directions if you don't have TechAdmin role.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedDirection = await sender.Send(new UpdateDirectionCommand(directionDto));

        return Ok(updatedDirection);
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
        if (!AuthorizationHelper.IsTechAdmin(User))
        {
            logger.LogError("You have no rights because you are not an admin");
            return StatusCode(403, "Forbidden to delete direction if you don't have TechAdmin role.");
        }

        this.ValidateId(id, localizer);

        var result = await sender.Send(new DeleteDirectionByIdCommand(id));
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
        var providers = await sender.Send(new GetProvidersByFilterQuery(filter));

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
        var token = await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        var result = await sender.Send(new BlockProviderCommand(providerBlockDto, token));

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
        var response = await sender.Send(new ValidateImportDataCommand(data));

        return Ok(response);
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
        var providersCsvData = await sender.Send(new ExportProvidersQuery());

        if (providersCsvData is null or { Length: 0 })
        {
            return NoContent();
        }

        return File(providersCsvData, MediaTypeNames.Text.Csv, "providers.csv");
    }
}