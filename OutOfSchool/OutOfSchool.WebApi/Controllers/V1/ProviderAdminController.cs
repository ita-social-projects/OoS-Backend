using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[HasPermission(Permissions.ProviderAdmins)]
public class ProviderAdminController : Controller
{
    private readonly IProviderAdminService providerAdminService;
    private readonly ILogger<ProviderAdminController> logger;
    private string path;
    private string userId;

    public ProviderAdminController(
        IProviderAdminService providerAdminService,
        ILogger<ProviderAdminController> logger)
    {
        this.providerAdminService = providerAdminService;
        this.logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        userId = GettingUserProperties.GetUserId(User);
    }

    /// <summary>
    /// Method for creating new ProviderAdmin.
    /// </summary>
    /// <param name="providerAdmin">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateProviderAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<ActionResult> Create(CreateProviderAdminDto providerAdmin)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (!ModelState.IsValid)
        {
            logger.LogError($"Input data was not valid for User(id): {userId}");

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await providerAdminService.CreateProviderAdminAsync(
                userId,
                providerAdmin,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            result =>
            {
                logger.LogInformation("Successfully created ProviderAdmin(id): {result.UserId} by User(id): {UserId}", result.UserId, userId);
                return Ok(result);
            });
    }

    /// <summary>
    /// Method for deleting ProviderAdmin.
    /// </summary>
    /// <param name="providerAdminId">Entity's id to delete.</param>
    /// <param name="providerId">Provider's id for which operation perform.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete]
    public async Task<ActionResult> Delete(string providerAdminId, Guid providerId)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        var response = await providerAdminService.DeleteProviderAdminAsync(
                providerAdminId,
                userId,
                providerId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match(
            error => StatusCode((int)error.HttpStatusCode),
            _ =>
            {
                logger.LogInformation($"Succesfully deleted ProviderAdmin(id): {providerAdminId} by User(id): {userId}.");

                return Ok();
            });
    }

    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut]
    public async Task<ActionResult> Block(string providerAdminId, Guid providerId)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        var response = await providerAdminService.BlockProviderAdminAsync(
                providerAdminId,
                userId,
                providerId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match(
            error => StatusCode((int)error.HttpStatusCode),
            _ =>
            {
                logger.LogInformation($"Succesfully blocked ProviderAdmin(id): {providerAdminId} by User(id): {userId}.");

                return Ok();
            });
    }

    /// <summary>
    /// Method to Get filtered data about related ProviderAdmins.
    /// </summary>
    /// <param name="deputyOnly">Returns only deputy provider admins.</param>
    /// <param name="assistantsOnly">Returns only assistants (workshop) provider admins.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProviderAdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.ProviderRead)]
    [HttpGet]
    public async Task<IActionResult> GetFilteredProviderAdminsAsync(bool deputyOnly, bool assistantsOnly)
    {
        var relatedAdmins = await providerAdminService.GetRelatedProviderAdmins(userId).ConfigureAwait(false);

        IActionResult result = Ok(relatedAdmins);

        if (assistantsOnly && deputyOnly)
        {
            return result;
        }
        else if (deputyOnly)
        {
            result = Ok(relatedAdmins.Where(w => w.IsDeputy));
        }
        else if (assistantsOnly)
        {
            result = Ok(relatedAdmins.Where(w => !w.IsDeputy));
        }

        return result;
    }

    /// <summary>
    /// Method to Get data about related ProviderAdmins.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProviderAdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetRelatedProviderAdmins()
    {
        var relatedAdmins = await providerAdminService.GetRelatedProviderAdmins(userId).ConfigureAwait(false);

        if (!relatedAdmins.Any())
        {
            return NoContent();
        }

        IActionResult result = Ok(relatedAdmins);

        return result;
    }

    /// <summary>
    /// Method to Get data about managed Workshops.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> ManagedWorkshops()
    {
        var userSubrole = GettingUserProperties.GetUserSubrole(HttpContext);

        if (userSubrole != Subrole.ProviderDeputy && userSubrole != Subrole.ProviderAdmin)
        {
            return BadRequest();
        }

        var relatedWorkshops = await providerAdminService.GetWorkshopsThatProviderAdminCanManage(userId, userSubrole == Subrole.ProviderDeputy).ConfigureAwait(false);

        if (!relatedWorkshops.Any())
        {
            return NoContent();
        }

        return Ok(relatedWorkshops);
    }
}