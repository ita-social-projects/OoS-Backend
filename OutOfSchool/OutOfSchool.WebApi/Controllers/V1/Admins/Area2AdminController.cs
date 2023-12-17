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
public class Area2AdminController : Controller
{
    private readonly Area2AdminService areaAdminService;
    private readonly ILogger<Area2AdminController> logger;
    private string userId;

    public Area2AdminController(
        Area2AdminService areaAdminService,
        ILogger<Area2AdminController> logger)
    {
        ArgumentNullException.ThrowIfNull(areaAdminService);
        ArgumentNullException.ThrowIfNull(logger);

        this.areaAdminService = areaAdminService;
        this.logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        userId = GettingUserProperties.GetUserId(User);
    }

    /// <summary>
    /// Get the Profile of authorized AreaAdmin.
    /// </summary>
    /// <returns>Authorized AreaAdmin's profile.</returns>
    [HttpGet]
    [HasPermission(Permissions.PersonalInfo)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Area2AdminDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Profile()
    {
        if (userId == null)
        {
            BadRequest("Invalid user information.");
        }

        try
        {
            return Ok(await areaAdminService.GetByUserIdAsync(userId));
        }
        catch (ArgumentException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get the AreaAdmin by it's id.
    /// </summary>
    /// <param name="id">Entity's id.</param>
    /// <returns>Authorized AreaAdmin's profile.</returns>
    [HttpGet]
    [HasPermission(Permissions.AreaAdminRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Area2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromQuery] string id)
    {
        logger.LogInformation("The getting the area admin according to the id {Id} was started.", id);

        var areaAdmin = await areaAdminService.GetByIdAsync(id);

        if (areaAdmin == null)
        {
            return NotFound($"There is no area admin in DB with {nameof(areaAdmin.Id)} - {id}");
        }

        return Ok(areaAdmin);
    }

    /// <summary>
    /// Get the AreaAdmins that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{Area2AdminDto}"/>, or no content.</returns>
    [HttpGet]
    [HasPermission(Permissions.AreaAdminRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<Area2AdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] Area2AdminFilter filter)
    {
        logger.LogInformation("The getting of the area admins according to the filter was started.");

        var areaAdmins = await areaAdminService.GetByFilter(filter);

        if (areaAdmins.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(areaAdmins);
    }

    /// <summary>
    /// Create the new AreaAdmin.
    /// </summary>
    /// <param name="areaAdminDto">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    [HasPermission(Permissions.AreaAdminAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Area2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Create(Area2AdminDto areaAdminDto)
    {
        logger.LogInformation("The creation of the area admin by the user {UserId} was started.", userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("The Input data was not valid for User(id): {UserId}", userId);

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await areaAdminService.CreateAsync(
            userId,
            areaAdminDto,
            await HttpContext.GetTokenAsync("access_token"));

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The area admin wasn't created by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The area admin {resultUserId} was successfully created by the user {UserId}", (result as Area2AdminDto).Id, userId);

                return Ok(result as Area2AdminDto);
            });
    }

    /// <summary>
    /// Update the AreaAdmin that already exists.
    /// </summary>
    /// <param name="areaAdminDto">Entity with new properties.</param>
    /// <returns>AreaAdmin's key.</returns>
    [HttpPut]
    [HasPermission(Permissions.AreaAdminEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Area2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(Area2AdminDto areaAdminDto)
    {
        logger.LogInformation("The updating of the area admin {areaAdminId} by the user {UserId} was started.", areaAdminDto.Id, userId);

        if (areaAdminDto == null)
        {
            return BadRequest("The area Admin is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await areaAdminService.UpdateAsync(
                userId,
                areaAdminDto,
                await HttpContext.GetTokenAsync("access_token"));

            return response.Match<ActionResult>(
                error =>
                {
                    logger.LogInformation("The area admin wasn't updated by the user {UserId}", userId);

                    return StatusCode((int)error.HttpStatusCode, error.Message);
                },
                result =>
                {
                    logger.LogInformation("The area admin {resultUserId} was successfully updated by the user {UserId}", (result as Area2AdminDto).Id, userId);

                    return Ok();
                });
        }
        catch (DbUpdateConcurrencyException e)
        {
            return BadRequest(e);
        }
    }

    /// <summary>
    /// Delete the AreaAdmin.
    /// </summary>
    /// <param name="areaAdminId">Entity's id to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete]
    [HasPermission(Permissions.AreaAdminRemove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(string areaAdminId)
    {
        logger.LogInformation("The deleting of the area admin {areaAdminId} by the user {UserId} was started.", areaAdminId, userId);

        var response = await areaAdminService.DeleteAsync(
                areaAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token"));

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The area admin wasn't deleted by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The area admin {areaAdminId} was successfully deleted by the user {UserId}", areaAdminId, userId);

                return Ok();
            });
    }

    /// <summary>
    /// Block the AreaAdmin.
    /// </summary>
    /// <param name="areaAdminId">Entity's id to delete.</param>
    /// <param name="isBlocked">Blocking status.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut]
    [HasPermission(Permissions.AreaAdminEdit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Block(string areaAdminId, bool? isBlocked)
    {
        logger.LogInformation("The blocking of the area admin {areaAdminId} by the user {UserId} was started.", areaAdminId, userId);

        if (isBlocked is null)
        {
            logger.LogDebug("The IsBlocked parameter is not specified");

            return BadRequest("The IsBlocked parameter is required");
        }

        var response = await areaAdminService.BlockAsync(
                areaAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token"),
                (bool)isBlocked);

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The area admin wasn't blocked by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The area admin {areaAdminId} was successfully blocked by the user {UserId}", areaAdminId, userId);

                return Ok();
            });
    }

    /// <summary>
    /// Send the new invitation to AreaAdmin.
    /// </summary>
    /// <param name="areaAdminId">Entity's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{areaAdminId}")]
    [HasPermission(Permissions.AreaAdminEdit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reinvite(string areaAdminId)
    {
        logger.LogInformation("The reinvitation of the area admin {areaAdminId} by the user {UserId} was started.", areaAdminId, userId);

        var response = await areaAdminService.ReinviteAsync(
                areaAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token"));

        if (response == null)
        {
            return NoContent();
        }

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The area admin wasn't reinvited by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The area admin {areaAdminId} was successfully reinvited by the user {UserId}", areaAdminId, userId);

                return Ok();
            });
    }
}
