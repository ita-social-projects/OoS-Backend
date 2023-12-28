using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Admins;
using OutOfSchool.WebApi.Services.Admins;

namespace OutOfSchool.WebApi.Controllers.V1.Admins;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class Region2AdminController : Controller
{
    private readonly Region2AdminService regionAdminService;
    private readonly ILogger<Region2AdminController> logger;
    private string userId;

    public Region2AdminController(
        Region2AdminService regionAdminService,
        ILogger<Region2AdminController> logger)
    {
        ArgumentNullException.ThrowIfNull(regionAdminService);
        ArgumentNullException.ThrowIfNull(logger);

        this.regionAdminService = regionAdminService;
        this.logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        userId = GettingUserProperties.GetUserId(User);
    }

    /// <summary>
    /// Get the Profile of authorized RegionAdmin.
    /// </summary>
    /// <returns>Authorized RegionAdmin's profile.</returns>
    [HttpGet]
    [HasPermission(Permissions.PersonalInfo)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Region2AdminDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Profile()
    {
        if (userId == null)
        {
            BadRequest("Invalid user information.");
        }

        try
        {
            return Ok(await regionAdminService.GetByUserIdAsync(userId));
        }
        catch (ArgumentException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get the RegionAdmin by it's id.
    /// </summary>
    /// <param name="id">Entity's id.</param>
    /// <returns>Authorized RegionAdmin's profile.</returns>
    [HttpGet]
    [HasPermission(Permissions.RegionAdminRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Region2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromQuery] string id)
    {
        logger.LogInformation("The getting the region admin according to the id {Id} was started.", id);

        var regionAdmin = await regionAdminService.GetByIdAsync(id);

        if (regionAdmin == null)
        {
            return NotFound($"There is no region admin in DB with {nameof(regionAdmin.UserId)} - {id}");
        }

        return Ok(regionAdmin);
    }

    /// <summary>
    /// Get RegionAdmins that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{Region2AdminDto}"/>, or no content.</returns>
    [HttpGet]
    [HasPermission(Permissions.RegionAdminRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<Region2AdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] Region2AdminFilter filter)
    {
        logger.LogInformation("The getting of the region admins according to the filter was started.");

        var regionAdmins = await regionAdminService.GetByFilter(filter);

        if (regionAdmins.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(regionAdmins);
    }

    /// <summary>
    /// Create the new RegionAdmin.
    /// </summary>
    /// <param name="regionAdminDto">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    [HasPermission(Permissions.RegionAdminAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Region2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Create(Region2AdminDto regionAdminDto)
    {
        logger.LogInformation("The creation of the region admin by the user {UserId} was started.", userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("The input data was not valid for the user {UserId}", userId);

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await regionAdminService.CreateAsync(
            userId,
            regionAdminDto,
            await HttpContext.GetTokenAsync("access_token"));

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The region admin wasn't created by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The region admin {resultUserId} was successfully created by the user {UserId}", result.UserId, userId);

                return Ok(result);
            });
    }

    /// <summary>
    /// Update the RegionAdmin that already exists.
    /// </summary>
    /// <param name="regionAdminDto">Entity with new properties.</param>
    /// <returns>RegionAdmin's key.</returns>
    [HttpPut]
    [HasPermission(Permissions.RegionAdminEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Region2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(Region2AdminDto regionAdminDto)
    {
        if (regionAdminDto == null)
        {
            return BadRequest("The regionAdmin is null.");
        }

        logger.LogInformation("The updating of the region admin {regionAdminId} by the user {UserId} was started.", regionAdminDto.UserId, userId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await regionAdminService.UpdateAsync(
                userId,
                regionAdminDto,
                await HttpContext.GetTokenAsync("access_token"));

            return response.Match<ActionResult>(
                error =>
                {
                    logger.LogInformation("The region admin wasn't updated by the user {UserId}", userId);

                    return StatusCode((int)error.HttpStatusCode, error.Message);
                },
                result =>
                {
                    logger.LogInformation("The region admin {resultUserId} was successfully updated by the user {UserId}", result.UserId, userId);

                    return Ok();
                });
        }
        catch (DbUpdateConcurrencyException e)
        {
            return BadRequest(e);
        }
    }

    /// <summary>
    /// Delete the RegionAdmin.
    /// </summary>
    /// <param name="regionAdminId">Entity's id to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete]
    [HasPermission(Permissions.RegionAdminRemove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(string regionAdminId)
    {
        logger.LogInformation("The deleting of the region admin {regionAdminId} by the user {UserId} was started.", regionAdminId, userId);

        var response = await regionAdminService.DeleteAsync(
            regionAdminId,
            userId,
            await HttpContext.GetTokenAsync("access_token"));

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The region admin wasn't deleted by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The region admin {regionAdminId} was successfully deleted by the user {UserId}", regionAdminId, userId);

                return Ok();
            });
    }

    /// <summary>
    /// Block the RegionAdmin.
    /// </summary>
    /// <param name="regionAdminId">Entity's id to block.</param>
    /// <param name="isBlocked">Blocking status.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut]
    [HasPermission(Permissions.RegionAdminBlock)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Block(string regionAdminId, bool? isBlocked)
    {
        logger.LogInformation("The blocking of the region admin {regionAdminId} by the user {UserId} was started.", regionAdminId, userId);

        if (isBlocked is null)
        {
            logger.LogDebug("The IsBlocked parameter is not specified");

            return BadRequest("The IsBlocked parameter is required");
        }

        var response = await regionAdminService.BlockAsync(
            regionAdminId,
            userId,
            await HttpContext.GetTokenAsync("access_token"),
            (bool)isBlocked);

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The region admin wasn't blocked by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The region admin {regionAdminId} was successfully blocked by the user {UserId}", regionAdminId, userId);

                return Ok();
            });
    }

    /// <summary>
    /// Send the new invitation to RegionAdmin.
    /// </summary>
    /// <param name="regionAdminId">Entity's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{regionAdminId}")]
    [HasPermission(Permissions.RegionAdminAddNew)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reinvite(string regionAdminId)
    {
        logger.LogInformation("The reinvitation of the region admin {regionAdminId} by the user {UserId} was started.", regionAdminId, userId);

        var response = await regionAdminService.ReinviteAsync(
            regionAdminId,
            userId,
            await HttpContext.GetTokenAsync("access_token"));

        if (response == null)
        {
            return NoContent();
        }

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The region admin wasn't reinvited by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The region admin {regionAdminId} was successfully reinvited by the user {UserId}", regionAdminId, userId);

                return Ok();
            });
    }
}
