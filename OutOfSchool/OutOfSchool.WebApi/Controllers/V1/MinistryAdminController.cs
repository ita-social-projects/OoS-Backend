using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[HasPermission(Permissions.MinistryAdmins)]
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
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (!ModelState.IsValid)
        {
            logger.LogError($"Input data was not valid for User(id): {userId}");

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await ministryAdminService.CreateMinistryAdminAsync(
                userId,
                ministryAdmin,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        if (response.IsSuccess)
        {
            CreateMinistryAdminDto ministryAdminDto = (CreateMinistryAdminDto)response.Result;

            logger.LogInformation($"Succesfully created MinistryAdmin(id): {ministryAdminDto.UserId} by User(id): {userId}.");

            return Ok(ministryAdminDto);
        }

        return StatusCode((int)response.HttpStatusCode);
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

        if (response.IsSuccess)
        {
            logger.LogInformation($"Succesfully deleted ministryAdmin(id): {ministryAdminId} by User(id): {userId}.");

            return Ok();
        }

        return StatusCode((int)response.HttpStatusCode);
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

        if (response.IsSuccess)
        {
            logger.LogInformation($"Succesfully blocked ministryAdmin(id): {ministryAdminId} by User(id): {userId}.");

            return Ok();
        }

        return StatusCode((int)response.HttpStatusCode);
    }
}