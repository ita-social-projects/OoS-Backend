﻿using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;
using OutOfSchool.WebApi.Services.ProviderServices;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for Achievement entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService achievementService;
    private readonly IProviderService providerService;
    private readonly IProviderAdminService providerAdminService;
    private readonly IWorkshopService workshopService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementController"/> class.
    /// </summary>
    /// <param name="service">Service for Achievement entity.</param>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="providerAdminService">Service for ProviderAdmin model.</param>
    /// <param name="workshopService">Service for Workshop model.</param>

    public AchievementController(
        IAchievementService service,
        IProviderService providerService,
        IProviderAdminService providerAdminService,
        IWorkshopService workshopService)
    {
        this.achievementService = service;
        this.providerAdminService = providerAdminService;
        this.providerService = providerService;
        this.workshopService = workshopService;
    }

    /// <summary>
    /// To recieve the Achievement with the defined id.
    /// </summary>
    /// <param name="id">Key of the Achievement in the table.</param>
    /// <returns><see cref="AchievementDto"/>.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="500">If any server error occures. For example: Id was wrong.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AchievementDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await achievementService.GetById(id).ConfigureAwait(false));
    }

    /// <summary>
    /// To recieve the Achievement list by Workshop id.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{AchievementDto}"/>.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="500">If any server error occures. For example: Id was wrong.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<AchievementDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByWorkshopId([FromQuery] AchievementsFilter filter)
    {
        var achievements = await achievementService.GetByFilter(filter).ConfigureAwait(false);

        if (achievements.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(achievements);
    }

    /// <summary>
    /// To create a new Achievement and add it to the DB.
    /// </summary>
    /// <param name="achievementDto">AchievementCreateDto object that will be added.</param>
    /// <returns>Achievement that was created.</returns>
    /// <response code="201">Achievement was successfully created.</response>
    /// <response code="400">Model is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(AchievementCreateDTO achievementDto)
    {
        var providerId = await providerService.GetProviderIdForWorkshopById(achievementDto.WorkshopId).ConfigureAwait(false);

        if (await providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false)
        {
            return StatusCode(403, "It is forbidden to add achievements to workshops at blocked providers");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(achievementDto.WorkshopId).ConfigureAwait(false);

        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to create achievement for another providers.");
        }

        try
        {
            achievementDto.Id = Guid.Empty;

            var achievement = await achievementService.Create(achievementDto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = achievement.Id, },
                achievement);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// To update Achievement entity that already exists.
    /// </summary>
    /// <param name="achievementDto">AchievementCreateDTO object with new properties.</param>
    /// <returns>Achievement that was updated.</returns>
    /// <response code="200">Achievement was successfully updated.</response>
    /// <response code="400">Model is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AchievementDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(AchievementCreateDTO achievementDto)
    {
        var providerId = await providerService.GetProviderIdForWorkshopById(achievementDto.WorkshopId).ConfigureAwait(false);

        if (await providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false)
        {
            return StatusCode(403, "It is forbidden to update the workshops achievements at blocked providers");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(achievementDto.WorkshopId).ConfigureAwait(false);

        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to update achievement, which are not related to you");
        }

        return Ok(await achievementService.Update(achievementDto).ConfigureAwait(false));
    }

    /// <summary>
    /// Delete the Achievement entity from DB.
    /// </summary>
    /// <param name="id">The key of the Achievement in table.</param>
    /// <returns>Status Code.</returns>
    /// <response code="204">Achievement was successfully deleted.</response>
    /// <response code="400">If some workshops assosiated with this Achievement.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {

        AchievementDto achievement;

        try
        {
            achievement = await achievementService.GetById(id).ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        var providerId = await providerService.GetProviderIdForWorkshopById(achievement.WorkshopId).ConfigureAwait(false);

        if (await providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false)
        {
            return StatusCode(403, "It is forbidden to delete the workshops achievements at blocked providers");
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(achievement.WorkshopId).ConfigureAwait(false);

        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to delete achievement, which are not related to you");
        }

        try
        {
            await achievementService.Delete(id).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<bool> IsUserProvidersOwnerOrAdmin(Guid workshopId)
    {
        if (!User.IsInRole(nameof(Role.Provider).ToLower()))
        {
            return false;
        }

        var userId = GettingUserProperties.GetUserId(User);
        var providerId = await workshopService.GetWorkshopProviderOwnerIdAsync(workshopId).ConfigureAwait(false);
        try
        {
            var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);
            if (providerId != provider?.Id)
            {
                return false;
            }
        }
        catch (ArgumentException)
        {
            var isUserRelatedAdmin = await providerAdminService.CheckUserIsRelatedProviderAdmin(userId, providerId, workshopId).ConfigureAwait(false);
            if (!isUserRelatedAdmin)
            {
                return false;
            }
        }

        return true;
    }
}