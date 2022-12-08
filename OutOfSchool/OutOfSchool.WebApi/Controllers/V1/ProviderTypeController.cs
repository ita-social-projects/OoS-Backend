using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ProviderTypeController:ControllerBase
{
    private readonly IProviderTypeService service;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialGroupController"/> class.
    /// </summary>
    /// <param name="service">Service for SocialGroup model.</param>
    /// <param name="localizer">Localizer.</param>
    public ProviderTypeController(IProviderTypeService service, IStringLocalizer<SharedResource> localizer)
    {
        this.service = service;
        this.localizer = localizer;
    }

    /// <summary>
    /// Get all Social Groups from the database.
    /// </summary>
    /// <returns>List of all Social Groups.</returns>
    [HasPermission(Permissions.ImpersonalDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProviderTypeDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var providerType = await service.GetAll().ConfigureAwait(false);

        if (!providerType.Any())
        {
            return NoContent();
        }

        return Ok(providerType);
    }

    /// <summary>
    /// Get Social Group by it's id.
    /// </summary>
    /// <param name="id">Social Group id.</param>
    /// <returns>Social Group.</returns>
    [HasPermission(Permissions.ImpersonalDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderTypeDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        this.ValidateId(id, localizer);

        return Ok(await service.GetById(id).ConfigureAwait(false));
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
    public async Task<IActionResult> Create(ProviderTypeDto dto)
    {
        var providerType = await service.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = providerType.Id, },
            providerType);
    }

    /// <summary>
    /// Update info about a Social Group in the database.
    /// </summary>
    /// <param name="dto">Social Group to update.</param>
    /// <returns>Social Group.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialGroupDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(ProviderTypeDto dto)
    {
        return Ok(await service.Update(dto).ConfigureAwait(false));
    }

    /// <summary>
    /// Delete a specific Social Group from the database.
    /// </summary>
    /// <param name="id">Social Group id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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