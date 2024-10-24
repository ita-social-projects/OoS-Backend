using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.Tag;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class TagController : ControllerBase
{
    private readonly ITagService service;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagController"/> class.
    /// </summary>
    /// <param name="service">Service for Tag model.</param>
    /// <param name="localizer">Localizer.</param>
    public TagController(ITagService service, IStringLocalizer<SharedResource> localizer)
    {
        this.service = service;
        this.localizer = localizer;
    }

    /// <summary>
    /// Get all Tags from the database.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all Tags.</returns>
    [HasPermission(Permissions.ImpersonalDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TagDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get(LocalizationType localization = LocalizationType.Ua)
    {
        var tag = await service.GetAll(localization).ConfigureAwait(false);

        if (!tag.Any())
        {
            return NoContent();
        }

        return Ok(tag);
    }

    /// <summary>
    /// Get Tag by it's id.
    /// </summary>
    /// <param name="id">Tag id.</param>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>Tag.</returns>
    [HasPermission(Permissions.ImpersonalDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagDto))]
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
    /// Add a new Tag to the database.
    /// </summary>
    /// <param name="dto">Tag entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TagCreate dto)
    {
        var tag = await service.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = tag.Id},
            tag);
    }

    /// <summary>
    /// Update info about a Tag in the database.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <param name="dto">Tag to update.</param>
    /// <returns>Tag.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(TagDto dto, LocalizationType localization = LocalizationType.Ua)
    {
        var tag = await service.Update(dto, localization).ConfigureAwait(false);

        if (tag == null)
        {
            return BadRequest(tag);
        }

        return Ok(tag);
    }

    /// <summary>
    /// Delete a specific Tag from the database.
    /// </summary>
    /// <param name="id">Tag id.</param>
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
