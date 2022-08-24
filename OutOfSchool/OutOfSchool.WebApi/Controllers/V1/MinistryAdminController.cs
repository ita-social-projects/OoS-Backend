using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Common;
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

        var ministryAdmin = await ministryAdminService.GetByUserId(userId).ConfigureAwait(false);

        if (ministryAdmin == null)
        {
            return NoContent();
        }

        return Ok(ministryAdmin);
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
    public async Task<IActionResult> GetById([FromQuery] long id)
    {
        var ministryAdmin = await ministryAdminService.GetById(id).ConfigureAwait(false);
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
    /// <param name="ministryAdmin">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateMinistryAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.MinistryAdminAddNew)]
    [HttpPost]
    public async Task<ActionResult> Create(CreateMinistryAdminDto ministryAdmin)
    {
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid for User(id): {UserId}", userId);

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await ministryAdminService.CreateMinistryAdminAsync(
                userId,
                ministryAdmin,
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
    /// <param name="ministryAdminDto">MinistryAdminDto object with new properties.</param>
    /// <returns>MinistryAdmin's key.</returns>
    [HasPermission(Permissions.UserEdit)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MinistryAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(MinistryAdminDto ministryAdminDto)
    {
        if (ministryAdminDto == null)
        {
            return BadRequest("MinistryAdmin is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (userId != ministryAdminDto.Id)
        {
            return StatusCode(403, "Forbidden to update another user.");
        }

        return Ok(await ministryAdminService.Update(ministryAdminDto).ConfigureAwait(false));
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