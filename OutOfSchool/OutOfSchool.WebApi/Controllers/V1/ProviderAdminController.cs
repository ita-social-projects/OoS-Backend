﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[HasPermission(Permissions.ProviderAdmins)]
public class ProviderAdminController : Controller
{
    private readonly IProviderAdminService providerAdminService;
    private readonly IUserService userService;
    private readonly IProviderService providerService;
    private readonly ILogger<ProviderAdminController> logger;
    private string path;
    private string userId;

    public ProviderAdminController(
        IProviderAdminService providerAdminService,
        IUserService userService,
        IProviderService providerService,
        ILogger<ProviderAdminController> logger)
    {
        this.providerAdminService = providerAdminService;
        this.userService = userService;
        this.providerService = providerService;
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

        if (providerAdmin == null)
        {
            return BadRequest("ProviderAdmin is null.");
        }

        if (await IsProviderBlocked(providerAdmin.ProviderId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to create the provider admin at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to create the provider admin by the blocked provider.");
        }

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
    /// Update info about the ProviderAdmin.
    /// </summary>
    /// <param name="providerId">Provider's id for which operation perform.</param>
    /// <param name="providerAdminModel">Entity to update.</param>
    /// <returns>Updated ProviderAdmin.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderAdminDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(Guid providerId, UpdateProviderAdminDto providerAdminModel)
    {
        if (providerAdminModel == null)
        {
            return BadRequest("ProviderAdmin is null.");
        }

        if (await IsProviderBlocked(providerId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to update the provider admin at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to update the provider admin by the blocked provider.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await providerAdminService.UpdateProviderAdminAsync(
                providerAdminModel,
                userId,
                providerId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

            return response.Match(
                error => StatusCode((int)error.HttpStatusCode),
                _ =>
                {
                    logger.LogInformation($"Can't change ProviderAdmin with such parameters.\n" +
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

        if (await IsProviderBlocked(providerId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to delete the provider admin at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to delete the provider admin by the blocked provider.");
        }

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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut]
    public async Task<ActionResult> Block(string providerAdminId, Guid providerId, bool? isBlocked)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (isBlocked is null)
        {
            logger.LogDebug("IsBlocked parameter is not specified");
            return BadRequest("IsBlocked parameter is required");
        }

        if (await IsProviderBlocked(providerId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to block the provider admin at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to block the provider admin by the blocked provider.");
        }

        var response = await providerAdminService.BlockProviderAdminAsync(
                providerAdminId,
                userId,
                providerId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false),
                (bool)isBlocked)
            .ConfigureAwait(false);

        return response.Match(
            error => StatusCode((int)error.HttpStatusCode),
            _ =>
            {
                logger.LogInformation($"Successfully blocked ProviderAdmin(id): {providerAdminId} by User(id): {userId}.");

                return Ok();
            });
    }

    /// <summary>
    /// Method to Get filtered data about related ProviderAdmins.
    /// </summary>
    /// <param name="filter">Filter to get a part of all provider admins that were found.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderAdminDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.ProviderRead)]
    [HttpGet]
    public async Task<IActionResult> GetFilteredProviderAdminsAsync([FromQuery] ProviderAdminSearchFilter filter)
    {
        var relatedAdmins = await providerAdminService.GetFilteredRelatedProviderAdmins(userId, filter).ConfigureAwait(false);

        if (relatedAdmins.TotalAmount == 0)
        {
            return NoContent();
        }

        return Ok(relatedAdmins);
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

        return Ok(relatedAdmins);
    }

    /// <summary>
    /// Method to Get data about managed Workshops.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopProviderViewCard>))]
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

        if (relatedWorkshops.TotalAmount == 0)
        {
            return NoContent();
        }

        return Ok(relatedWorkshops);
    }

    /// <summary>
    /// Get ProviderAdmin by its id.
    /// </summary>
    /// <param name="providerAdminId">ProviderAdmin's id.</param>
    /// <returns>Info about ProviderAdmin.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FullProviderAdminDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{providerAdminId}")]
    public async Task<IActionResult> GetProviderAdminById(string providerAdminId)
    {
        var providerAdmin = await providerAdminService.GetFullProviderAdmin(providerAdminId)
            .ConfigureAwait(false);
        if (providerAdmin == null)
        {
            return NoContent();
        }

        return Ok(providerAdmin);
    }

    /// <summary>
    /// Send new invitation to ProviderAdmin.
    /// </summary>
    /// <param name="providerAdminId">ProviderAdmin's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{providerAdminId}")]
    public async Task<IActionResult> Reinvite(string providerAdminId)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to reinvite the provider admin by the blocked provider.");
        }

        var response = await providerAdminService.ReinviteProviderAdminAsync(
                providerAdminId,
                userId,
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
                logger.LogInformation($"Succesfully deleted ProviderAdmin(id): {providerAdminId} by User(id): {userId}.");

                return Ok();
            });
    }

    private async Task<bool> IsCurrentUserBlocked() =>
        await userService.IsBlocked(userId);

    private async Task<bool> IsProviderBlocked(Guid providerId) =>
        await providerService.IsBlocked(providerId).ConfigureAwait(false);

}
