using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class AreaAdminController : Controller
{
    private readonly IAreaAdminService areaAdminService;
    private readonly ILogger<AreaAdminController> logger;
    private string currentUserId;
    private string currentUserRole;

    public AreaAdminController(
        IAreaAdminService areaAdminService,
        ILogger<AreaAdminController> logger)
    {
        ArgumentNullException.ThrowIfNull(areaAdminService);
        ArgumentNullException.ThrowIfNull(logger);

        this.areaAdminService = areaAdminService;
        this.logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        currentUserId = GettingUserProperties.GetUserId(User);
        currentUserRole = GettingUserProperties.GetUserRole(User);
    }

    /// <summary>
    /// To Get the Profile of authorized AreaAdmin.
    /// </summary>
    /// <returns>Authorized AreaAdmin's profile.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AreaAdminDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.PersonalInfo)]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (currentUserId == null)
        {
            return BadRequest("Invalid user information.");
        }

        try
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserId).ConfigureAwait(false);
            return Ok(areaAdmin);
        }
        catch (ArgumentException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get AreaAdmin by it's id.
    /// </summary>
    /// <param name="id">AreaAdmin id.</param>
    /// <returns>Authorized AreaAdmin's profile.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AreaAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.AreaAdminRead)]
    [HttpGet]
    public async Task<IActionResult> GetById([FromQuery] string id)
    {
        var areaAdmin = await areaAdminService.GetByIdAsync(id).ConfigureAwait(false);
        if (areaAdmin == null)
        {
            return NotFound($"There is no AreaAdmin in DB with {nameof(areaAdmin.Id)} - {id}");
        }

        return Ok(areaAdmin);
    }

    /// <summary>
    /// Get AreaAdmins that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{AreaAdminDto}"/>, or no content.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<AreaAdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.AreaAdminEdit)]
    [HttpGet]
    public async Task<IActionResult> GetByFilter([FromQuery] AreaAdminFilter filter)
    {
        var areaAdmins = await areaAdminService.GetByFilter(filter).ConfigureAwait(false);

        if (areaAdmins.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(areaAdmins);
    }

    /// <summary>
    /// Method for creating new AreaAdmin.
    /// </summary>
    /// <param name="areaAdminBase">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AreaAdminBaseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.AreaAdminAddNew)]
    [HttpPost]
    public async Task<ActionResult> Create(AreaAdminBaseDto areaAdminBase)
    {
        logger.LogDebug("{User(id): {UserId}", currentUserId);

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid for User(id): {UserId}", currentUserId);

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        if (currentUserRole == nameof(Role.MinistryAdmin).ToLower()
             && !await areaAdminService.IsAreaAdminSubordinateMinistryCreateAsync(currentUserId, areaAdminBase.InstitutionId))
        {
            logger.LogDebug("Forbidden to create AreaAdmin. AreaAdmin doesn't subordinate to MinistryAdmin.");
            return StatusCode(403,
                "Forbidden to create AreaAdmin. AreaAdmin doesn't subordinate to MinistryAdmin.");
        }

        if (currentUserRole == nameof(Role.RegionAdmin).ToLower()
             && !await areaAdminService.IsAreaAdminSubordinateRegionCreateAsync(currentUserId, areaAdminBase.InstitutionId, areaAdminBase.CATOTTGId))
        {
            logger.LogDebug("Forbidden to create AreaAdmin. AreaAdmin doesn't subordinate to RegionAdmin.");
            return StatusCode(403,
                "Forbidden to create AreaAdmin. AreaAdmin doesn't subordinate to RegionAdmin.");
        }

        var response = await areaAdminService.CreateAreaAdminAsync(
                currentUserId,
                areaAdminBase,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            result =>
            {
                logger.LogInformation("Successfully created AreaAdmin(id): {result.UserId} by User(id): {UserId}",
                    result.UserId, currentUserId);
                return Ok(result);
            });
    }

    /// <summary>
    /// To update AreaAdmin entity that already exists.
    /// </summary>
    /// <param name="updateAreaAdminDto">AreaAdminDto object with new properties.</param>
    /// <returns>AreaAdmin's key.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AreaAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.AreaAdminEdit)]
    [HttpPut]
    public async Task<ActionResult> Update(AreaAdminDto updateAreaAdminDto)
    {
        if (updateAreaAdminDto == null)
        {
            return BadRequest("AreaAdmin is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (currentUserId != updateAreaAdminDto.Id)
        {
            if (!(currentUserRole == nameof(Role.TechAdmin).ToLower() ||
                  currentUserRole == nameof(Role.MinistryAdmin).ToLower()))
            {
                logger.LogDebug("Forbidden to update another user if you don't have TechAdmin or MinistryAdmin role.");
                return StatusCode(403,
                    "Forbidden to update another user if you don't have TechAdmin or MinistryAdmin role.");
            }

            if ((currentUserRole == nameof(Role.MinistryAdmin).ToLower()
                && !await areaAdminService.IsAreaAdminSubordinateMinistryAsync(currentUserId, updateAreaAdminDto.Id)) ||
                (currentUserRole == nameof(Role.RegionAdmin).ToLower()
                 && !await areaAdminService.IsAreaAdminSubordinateRegionAsync(currentUserId, updateAreaAdminDto.Id)))
            {
                logger.LogDebug("Forbidden to update AreaAdmin. AreaAdmin doesn't subordinate to MinistryAdmin.");
                return StatusCode(403,
                    "Forbidden to update AreaAdmin. AreaAdmin doesn't subordinate to MinistryAdmin.");
            }

            var updatedRegionAdmin = await areaAdminService.GetByIdAsync(updateAreaAdminDto.Id);
            if (updatedRegionAdmin.AccountStatus == AccountStatus.Accepted)
            {
                logger.LogDebug("Forbidden to update accepted user.");
                return StatusCode(403, "Forbidden to update accepted user.");
            }
        }

        try
        {
            var response = await areaAdminService.UpdateAreaAdminAsync(
                    currentUserId,
                    updateAreaAdminDto,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                .ConfigureAwait(false);

            return response.Match(
                error => StatusCode((int)error.HttpStatusCode),
                _ =>
                {
                    logger.LogInformation($"Can't update AreaAdmin with such parameters.\n" +
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
    /// Method for deleting AreaAdmin.
    /// </summary>
    /// <param name="areaAdminId">Entity's id to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HasPermission(Permissions.AreaAdminRemove)]
    [HttpDelete]
    public async Task<ActionResult> Delete(string areaAdminId)
    {
        logger.LogDebug($"User(id): {currentUserId}.");

        if ((currentUserRole == nameof(Role.MinistryAdmin).ToLower()
             && !await areaAdminService.IsAreaAdminSubordinateMinistryAsync(currentUserId, areaAdminId)) ||
            (currentUserRole == nameof(Role.RegionAdmin).ToLower()
             && !await areaAdminService.IsAreaAdminSubordinateRegionAsync(currentUserId, areaAdminId)))
        {
            logger.LogDebug("Forbidden to update AreaAdmin. AreaAdmin doesn't subordinate to MinistryAdmin.");
            return StatusCode(403,
                "Forbidden to update AreaAdmin. AreaAdmin doesn't subordinate to MinistryAdmin.");
        }

        var response = await areaAdminService.DeleteAreaAdminAsync(
                areaAdminId,
                currentUserId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            _ =>
            {
                logger.LogInformation(
                    $"Succesfully deleted AreaAdmin(id): {areaAdminId} by User(id): {currentUserId}.");
                return Ok();
            });
    }

    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HasPermission(Permissions.AreaAdminBlock)]
    [HttpPut]
    public async Task<ActionResult> Block(string areaAdminId, bool? isBlocked)
    {
        logger.LogDebug($"User(id): {currentUserId}.");

        if ((currentUserRole == nameof(Role.MinistryAdmin).ToLower()
             && !await areaAdminService.IsAreaAdminSubordinateMinistryAsync(currentUserId, areaAdminId)) ||
            (currentUserRole == nameof(Role.RegionAdmin).ToLower()
             && !await areaAdminService.IsAreaAdminSubordinateRegionAsync(currentUserId, areaAdminId)))
        {
            logger.LogDebug("Forbidden to update AreaAdmin. AreaAdmin doesn't subordinate to MinistryAdmin.");
            return StatusCode(403,
                "Forbidden to update AreaAdmin. AreaAdmin doesn't subordinate to MinistryAdmin.");
        }

        if (isBlocked is null)
        {
            logger.LogDebug("IsBlocked parameter is not specified");
            return BadRequest("IsBlocked parameter is required");
        }

        var response = await areaAdminService.BlockAreaAdminAsync(
                areaAdminId,
                currentUserId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false),
                (bool)isBlocked)
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            _ =>
            {
                logger.LogInformation(
                    $"Successfully blocked AreaAdmin(id): {areaAdminId} by User(id): {currentUserId}.");
                return Ok();
            });
    }

    /// <summary>
    /// Send new invitation to AreaAdmin.
    /// </summary>
    /// <param name="AreaAdminId">AreaAdmin's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.AreaAdminEdit)]
    [HttpPut("{regionAdminId}")]
    public async Task<IActionResult> Reinvite(string areaAdminId)
    {
        logger.LogDebug($"User(id): {currentUserId}.");

        var response = await areaAdminService.ReinviteAreaAdminAsync(
                areaAdminId,
                currentUserId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        if (response == null)
        {
            return NoContent();
        }

        return response.Match(
            error => StatusCode((int)error.HttpStatusCode),
            _ =>
            {
                logger.LogInformation(
                    $"Succesfully reinvited AreaAdmin(id): {areaAdminId} by User(id): {currentUserId}.");

                return Ok();
            });
    }
}