using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class RegionAdminController : Controller
{
    private readonly IRegionAdminService regionAdminService;
    private readonly ILogger<RegionAdminController> logger;
    private string path;
    private string userId;

    public RegionAdminController(
        IRegionAdminService regionAdminService,
        ILogger<RegionAdminController> logger)
    {
        ArgumentNullException.ThrowIfNull(regionAdminService);
        ArgumentNullException.ThrowIfNull(logger);

        this.regionAdminService = regionAdminService;
        this.logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        userId = GettingUserProperties.GetUserId(User);
    }

    /// <summary>
    /// To Get the Profile of authorized RegionAdmin.
    /// </summary>
    /// <returns>Authorized RegionAdmin's profile.</returns>
    [HasPermission(Permissions.PersonalInfo)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegionAdminDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Profile()
    {
        if (userId == null)
        {
            BadRequest("Invalid user information.");
        }

        try
        {
            var regionAdmin = await regionAdminService.GetByUserId(userId).ConfigureAwait(false);
            return Ok(regionAdmin);
        }
        catch (ArgumentException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get RegionAdmin by it's id.
    /// </summary>
    /// <param name="id">RegionAdmin id.</param>
    /// <returns>Authorized RegionAdmin's profile.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegionAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromQuery] string id)
    {
        var regionAdmin = await regionAdminService.GetByIdAsync(id).ConfigureAwait(false);
        if (regionAdmin == null)
        {
            return NotFound($"There is no Region admin in DB with {nameof(regionAdmin.Id)} - {id}");
        }

        return Ok(regionAdmin);
    }

    /// <summary>
    /// Get RegionAdmins that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{RegionAdminDto}"/>, or no content.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<RegionAdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] RegionAdminFilter filter)
    {
        var regionAdmins = await regionAdminService.GetByFilter(filter).ConfigureAwait(false);

        if (regionAdmins.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(regionAdmins);
    }

    /// <summary>
    /// Method for creating new RegionAdmin.
    /// </summary>
    /// <param name="regionAdminBase">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RegionAdminBaseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.RegionAdminAddNew)]
    [HttpPost]
    public async Task<ActionResult> Create(RegionAdminBaseDto regionAdminBase)
    {
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid for User(id): {UserId}", userId);

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await regionAdminService.CreateRegionAdminAsync(
                userId,
                regionAdminBase,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            result =>
            {
                logger.LogInformation("Successfully created RegionAdmin(id): {result.UserId} by User(id): {UserId}", result.UserId, userId);
                return Ok(result);
            });
    }

    /// <summary>
    /// To update RegionAdmin entity that already exists.
    /// </summary>
    /// <param name="updateRegionAdminDto">RegionAdminDto object with new properties.</param>
    /// <returns>RegionAdmin's key.</returns>
    [HasPermission(Permissions.UserEdit)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegionAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(RegionAdminBaseDto updateRegionAdminDto)
    {
        if (updateRegionAdminDto == null)
        {
            return BadRequest("RegionAdmin is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (userId != updateRegionAdminDto.UserId)
        {
            var currentUser = GettingUserProperties.GetUserId(User);
            var currentUserRole = GettingUserProperties.GetUserRole(User);

            // TODO Add checking if Region admin belongs to the same Institution as Ministry Admin
            if (currentUserRole == nameof(Role.TechAdmin).ToLower()
                || currentUserRole == nameof(Role.MinistryAdmin).ToLower())
            {
                var updatedRegionAdmin = await regionAdminService.GetByIdAsync(updateRegionAdminDto.UserId);
                if (updatedRegionAdmin.AccountStatus == AccountStatus.Accepted)
                {
                    return StatusCode(403, "Forbidden to update accepted user.");
                }
            }
            else
            {
                return StatusCode(403, "Forbidden to update another user if you haven't techadmin or ministryadmin role.");
            }
        }

        try
        {
            var response = await regionAdminService.UpdateRegionAdminAsync(
                    userId,
                    updateRegionAdminDto,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                .ConfigureAwait(false);

            return response.Match(
                error => StatusCode((int)error.HttpStatusCode),
                _ =>
                {
                    logger.LogInformation($"Can't update RegionAdmin with such parameters.\n" +
                                          "Please check that information are valid.");

                    return Ok();
                });
        }
        catch (DbUpdateConcurrencyException e)
        {
            return BadRequest(e);
        }
    }

    /// <summary>
    /// Method for deleting RegionAdmin.
    /// </summary>
    /// <param name="regionAdminId">Entity's id to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HasPermission(Permissions.RegionAdminRemove)]
    [HttpDelete]
    public async Task<ActionResult> Delete(string regionAdminId)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        var response = await regionAdminService.DeleteRegionAdminAsync(
                regionAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            _ =>
            {
                logger.LogInformation($"Succesfully deleted regionAdmin(id): {regionAdminId} by User(id): {userId}.");
                return Ok();
            });
    }

    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HasPermission(Permissions.RegionAdminEdit)]
    [HttpPut]
    public async Task<ActionResult> Block(string regionAdminId, bool? isBlocked)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (isBlocked is null)
        {
            logger.LogDebug("IsBlocked parameter is not specified");
            return BadRequest("IsBlocked parameter is required");
        }

        var response = await regionAdminService.BlockRegionAdminAsync(
                regionAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false),
                (bool)isBlocked)
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            _ =>
            {
                logger.LogInformation($"Successfully blocked regionAdmin(id): {regionAdminId} by User(id): {userId}.");
                return Ok();
            });
    }
}