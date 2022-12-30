using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class SocialGroupController : ControllerBase
{
    private readonly ISocialGroupService service;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialGroupController"/> class.
    /// </summary>
    /// <param name="service">Service for SocialGroup model.</param>
    /// <param name="localizer">Localizer.</param>
    public SocialGroupController(ISocialGroupService service, IStringLocalizer<SharedResource> localizer)
    {
        this.service = service;
        this.localizer = localizer;
    }

    /// <summary>
    /// Get all Social Groups from the database.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all Social Groups.</returns>
    [HasPermission(Permissions.ImpersonalDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SocialGroupDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get(LocalizationType localization = LocalizationType.Ua)
    {
        var socialGroup = await service.GetAll(localization).ConfigureAwait(false);

        if (!socialGroup.Any())
        {
            return NoContent();
        }

        return Ok(socialGroup);
    }

    /// <summary>
    /// Get Social Group by it's id.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <param name="id">Social Group id.</param>
    /// <returns>Social Group.</returns>
    [HasPermission(Permissions.ImpersonalDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialGroupDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, LocalizationType localization = LocalizationType.Ua)
    {
        this.ValidateId(id, localizer);

        return Ok(await service.GetById(id, localization).ConfigureAwait(false));
    }

    /// <summary>
    /// Add a new Social Group to the database.
    /// </summary>
    /// <param name="dto">Social Group entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create(SocialGroupCreate dto)
    {
        var socialGroup = await service.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = socialGroup.Id, },
            socialGroup);
    }

    /// <summary>
    /// Update info about a Social Group in the database.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <param name="dto">Social Group to update.</param>
    /// <returns>Social Group.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialGroupDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(SocialGroupDto dto, LocalizationType localization = LocalizationType.Ua)
    {
        var socialGroup = await service.Update(dto, localization).ConfigureAwait(false);

        if (socialGroup == null)
        {
            return BadRequest(socialGroup);
        }

        return Ok(socialGroup);
    }

    /// <summary>
    /// Delete a specific Social Group from the database.
    /// </summary>
    /// <param name="id">Social Group id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        this.ValidateId(id, localizer);

        await service.Delete(id).ConfigureAwait(false);

        return NoContent();
    }
}