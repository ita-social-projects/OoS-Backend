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
public class Ministry2AdminController : Controller
{
    private readonly Ministry2AdminService ministryAdminService;
    private readonly ILogger<Ministry2AdminController> logger;
    private string userId;

    public Ministry2AdminController(
        Ministry2AdminService ministryAdminService,
        ILogger<Ministry2AdminController> logger)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminService);
        ArgumentNullException.ThrowIfNull(logger);

        this.ministryAdminService = ministryAdminService;
        this.logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        userId = GettingUserProperties.GetUserId(User);
    }

    /// <summary>
    /// Get the Profile of authorized MinistryAdmin.
    /// </summary>
    /// <returns>Authorized MinistryAdmin's profile.</returns>
    [HttpGet]
    [HasPermission(Permissions.PersonalInfo)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Ministry2AdminDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Profile()
    {
        if (userId == null)
        {
            BadRequest("Invalid user information.");
        }

        try
        {
            return Ok(await ministryAdminService.GetByUserIdAsync(userId));
        }
        catch (ArgumentException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get the MinistryAdmin by it's id.
    /// </summary>
    /// <param name="id">Entity's id.</param>
    /// <returns>Authorized MinistryAdmin's profile.</returns>
    [HttpGet]
    [HasPermission(Permissions.MinistryAdminRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Ministry2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromQuery] string id)
    {
        logger.LogInformation("The getting the ministry admin according to the id {Id} was started.", id);

        var ministryAdmin = await ministryAdminService.GetByIdAsync(id);

        if (ministryAdmin == null)
        {
            return NotFound($"There is no ministry admin in DB with {nameof(ministryAdmin.UserId)} - {id}");
        }

        return Ok(ministryAdmin);
    }

    /// <summary>
    /// Get MinistryAdmins that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{Ministry2AdminDto}"/>, or no content.</returns>
    [HttpGet]
    [HasPermission(Permissions.MinistryAdminRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<Ministry2AdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] Ministry2AdminFilter filter)
    {
        logger.LogInformation("The getting of the ministry admins according to the filter was started.");

        var ministryAdmins = await ministryAdminService.GetByFilter(filter);

        if (ministryAdmins.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(ministryAdmins);
    }

    /// <summary>
    /// Create the new MinistryAdmin.
    /// </summary>
    /// <param name="ministryAdminDto">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    [HasPermission(Permissions.MinistryAdminAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Ministry2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Create(Ministry2AdminDto ministryAdminDto)
    {
        logger.LogInformation("The creation of the ministry admin by the user {UserId} was started.", userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("The input data was not valid for the user {UserId}", userId);

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await ministryAdminService.CreateAsync(
            userId,
            ministryAdminDto,
            await HttpContext.GetTokenAsync("access_token"));

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The ministry admin wasn't created by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The ministry admin {resultUserId} was successfully created by the user {UserId}", result.UserId, userId);

                return Ok(result);
            });
    }

    /// <summary>
    /// Update the MinistryAdmin that already exists.
    /// </summary>
    /// <param name="ministryAdminDto">Entity with new properties.</param>
    /// <returns>MinistryAdmin's key.</returns>
    [HttpPut]
    [HasPermission(Permissions.MinistryAdminEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Ministry2AdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(Ministry2AdminDto ministryAdminDto)
    {
        if (ministryAdminDto == null)
        {
            return BadRequest("The ministry admin is null.");
        }

        logger.LogInformation("The updating of the ministry admin {ministryAdminId} by the user {UserId} was started.", ministryAdminDto.UserId, userId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await ministryAdminService.UpdateAsync(
                    userId,
                    ministryAdminDto,
                    await HttpContext.GetTokenAsync("access_token"));

            return response.Match<ActionResult>(
                error =>
                {
                    logger.LogInformation("The ministry admin wasn't updated by the user {UserId}", userId);

                    return StatusCode((int)error.HttpStatusCode, error.Message);
                },
                result =>
                {
                    logger.LogInformation("The ministry admin {resultUserId} was successfully updated by the user {UserId}", result.UserId, userId);

                    return Ok();
                });
        }
        catch (DbUpdateConcurrencyException e)
        {
            return BadRequest(e);
        }
    }

    /// <summary>
    /// Delete the MinistryAdmin.
    /// </summary>
    /// <param name="ministryAdminId">Entity's id to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete]
    [HasPermission(Permissions.MinistryAdminRemove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(string ministryAdminId)
    {
        logger.LogInformation("The deleting of the ministry admin {ministryAdminId} by the user {UserId} was started.", ministryAdminId, userId);

        var response = await ministryAdminService.DeleteAsync(
                ministryAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token"));

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The ministry admin wasn't deleted by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The ministry admin {ministryAdminId} was successfully deleted by the user {UserId}", ministryAdminId, userId);

                return Ok();
            });
    }

    /// <summary>
    /// Block the MinistryAdmin.
    /// </summary>
    /// <param name="ministryAdminId">Entity's id to block.</param>
    /// <param name="isBlocked">Blocking status.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut]
    [HasPermission(Permissions.MinistryAdminBlock)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Block(string ministryAdminId, bool? isBlocked)
    {
        logger.LogInformation("The blocking of the ministry admin {ministryAdminId} by the user {UserId} was started.", ministryAdminId, userId);

        if (isBlocked is null)
        {
            logger.LogDebug("The IsBlocked parameter is not specified");

            return BadRequest("The IsBlocked parameter is required");
        }

        var response = await ministryAdminService.BlockAsync(
                ministryAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token"),
                (bool)isBlocked);

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The ministry admin wasn't blocked by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The ministry admin {ministryAdminId} was successfully blocked by the user {UserId}", ministryAdminId, userId);

                return Ok();
            });
    }

    /// <summary>
    /// Send the new invitation to the MinistryAdmin.
    /// </summary>
    /// <param name="ministryAdminId">Entity's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{ministryAdminId}")]
    [HasPermission(Permissions.MinistryAdminEdit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reinvite(string ministryAdminId)
    {
        logger.LogInformation("The reinvitation of the ministry admin {ministryAdminId} by the user {UserId} was started.", ministryAdminId, userId);

        var response = await ministryAdminService.ReinviteAsync(
                ministryAdminId,
                userId,
                await HttpContext.GetTokenAsync("access_token"));

        if (response == null)
        {
            return NoContent();
        }

        return response.Match<ActionResult>(
            error =>
            {
                logger.LogInformation("The ministry admin wasn't reinvited by the user {UserId}", userId);

                return StatusCode((int)error.HttpStatusCode, error.Message);
            },
            result =>
            {
                logger.LogInformation("The ministry admin {ministryAdminId} was successfully reinvited by the user {UserId}", ministryAdminId, userId);

                return Ok();
            });
    }
}