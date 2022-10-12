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
public class MinistryAdminController : Controller
{
    private readonly IMinistryAdminService ministryAdminService;
    private readonly ILogger<MinistryAdminController> logger;
    private string path;
    private string userId;

    public MinistryAdminController(
        IMinistryAdminService ministryAdminService,
        ILogger<MinistryAdminController> logger)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminService);
        ArgumentNullException.ThrowIfNull(logger);

        this.ministryAdminService = ministryAdminService;
        this.logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        userId = GettingUserProperties.GetUserId(User);
    }

    /// <summary>
    /// To Get the Profile of authorized MinistryAdmin.
    /// </summary>
    /// <returns>Authorized MinistryAdmin's profile.</returns>
    [HasPermission(Permissions.UserRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MinistryAdminDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Profile()
    {
        if (userId == null)
        {
            BadRequest("Invalid user information.");
        }

        try
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(userId).ConfigureAwait(false);
            return Ok(ministryAdmin);
        }
        catch (ArgumentException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get MinistryAdmin by it's id.
    /// </summary>
    /// <param name="id">MinistryAdmin id.</param>
    /// <returns>Authorized MinistryAdmin's profile.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MinistryAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromQuery] string id)
    {
        var ministryAdmin = await ministryAdminService.GetByIdAsync(id).ConfigureAwait(false);
        if (ministryAdmin == null)
        {
            return NotFound($"There is no Ministry admin in DB with {nameof(ministryAdmin.Id)} - {id}");
        }

        return Ok(ministryAdmin);
    }

    /// <summary>
    /// Get MinistryAdmins that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{MinistryAdminDto}"/>, or no content.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<MinistryAdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] MinistryAdminFilter filter)
    {
        var ministryAdmins = await ministryAdminService.GetByFilter(filter).ConfigureAwait(false);

        if (ministryAdmins.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(ministryAdmins);
    }

    /// <summary>
    /// Method for creating new MinistryAdmin.
    /// </summary>
    /// <param name="ministryAdminBase">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MinistryAdminBaseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.MinistryAdminAddNew)]
    [HttpPost]
    public async Task<ActionResult> Create(MinistryAdminBaseDto ministryAdminBase)
    {
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid for User(id): {UserId}", userId);

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await ministryAdminService.CreateMinistryAdminAsync(
                userId,
                ministryAdminBase,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            result =>
            {
                logger.LogInformation("Successfully created MinistryAdmin(id): {result.UserId} by User(id): {UserId}", result.UserId, userId);
                return Ok(result);
            });
    }

    /// <summary>
    /// To update MinistryAdmin entity that already exists.
    /// </summary>
    /// <param name="updateMinistryAdminDto">MinistryAdminDto object with new properties.</param>
    /// <returns>MinistryAdmin's key.</returns>
    [HasPermission(Permissions.UserEdit)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MinistryAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(MinistryAdminBaseDto updateMinistryAdminDto)
    {
        if (updateMinistryAdminDto == null)
        {
            return BadRequest("MinistryAdmin is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (userId != updateMinistryAdminDto.UserId)
        {
            var currentUserRole = GettingUserProperties.GetUserRole(User);
            if (currentUserRole == nameof(Role.TechAdmin).ToLower())
            {
                var updatedMinistryAdmin = await ministryAdminService.GetByIdAsync(updateMinistryAdminDto.UserId);
                if (updatedMinistryAdmin.AccountStatus == AccountStatus.Accepted)
                {
                    return StatusCode(403, "Forbidden to update accepted user.");
                }
            }
            else
            {
                return StatusCode(403, "Forbidden to update another user if you haven't techadmin role.");
            }
        }

        try
        {
            var response = await ministryAdminService.UpdateMinistryAdminAsync(
                    userId,
                    updateMinistryAdminDto,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                .ConfigureAwait(false);

            return response.Match(
                error => StatusCode((int)error.HttpStatusCode),
                _ =>
                {
                    logger.LogInformation($"Can't update MinistryAdmin with such parameters.\n" +
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
    /// Method for deleting MinistryAdmin.
    /// </summary>
    /// <param name="ministryAdminId">Entity's id to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HasPermission(Permissions.MinistryAdminRemove)]
    [HttpDelete]
    public async Task<ActionResult> Delete(string ministryAdminId)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        var response = await ministryAdminService.DeleteMinistryAdminAsync(
                ministryAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            _ =>
            {
                logger.LogInformation($"Succesfully deleted ministryAdmin(id): {ministryAdminId} by User(id): {userId}.");
                return Ok();
            });
    }

    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HasPermission(Permissions.MinistryAdminEdit)]
    [HttpPut]
    public async Task<ActionResult> Block(string ministryAdminId)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        var response = await ministryAdminService.BlockMinistryAdminAsync(
                ministryAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            _ =>
            {
                logger.LogInformation($"Succesfully blocked ministryAdmin(id): {ministryAdminId} by User(id): {userId}.");
                return Ok();
            });
    }
}