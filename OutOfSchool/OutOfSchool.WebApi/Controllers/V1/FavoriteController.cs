using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService service;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoriteController"/> class.
    /// </summary>
    /// <param name="service">Service for Favorite model.</param>
    /// <param name="localizer">Localizer.</param>
    public FavoriteController(IFavoriteService service, IStringLocalizer<SharedResource> localizer)
    {
        this.service = service;
        this.localizer = localizer;
    }

    /// <summary>
    /// Get all Favorites from the database.
    /// </summary>
    /// <returns>List of all Favorites.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FavoriteDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("all")]
    public async Task<IActionResult> Get()
    {
        var favorites = await service.GetAll().ConfigureAwait(false);

        if (!favorites.Any())
        {
            return NoContent();
        }

        return Ok(favorites);
    }

    /// <summary>
    /// Get Favorite by it's id.
    /// </summary>
    /// <param name="id">Favorite id.</param>
    /// <returns>Favorite.</returns>
    [HasPermission(Permissions.FavoriteRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FavoriteDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        this.ValidateId(id, localizer);

        return Ok(await service.GetById(id).ConfigureAwait(false));
    }

    /// <summary>
    /// Get all Favorites from the database by UserId.
    /// </summary>
    /// <returns>List of all User Favorites.</returns>
    [HasPermission(Permissions.FavoriteRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FavoriteDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetAllByUser()
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var favorites = await service.GetAllByUser(userId).ConfigureAwait(false);

        if (!favorites.Any())
        {
            return NoContent();
        }

        return Ok(favorites);
    }

    /// <summary>
    /// Get all Favorites workshops from the database by UserId.
    /// </summary>
    /// <param name="offsetFilter">Filter to get spesified portion of entities.</param>
    /// <returns>List of all User favorite Workshops.</returns>
    [HasPermission(Permissions.FavoriteRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("workshops")]
    public async Task<IActionResult> GetFavoriteWorkshopsByUser([FromQuery] OffsetFilter offsetFilter)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var favorites = await service.GetFavoriteWorkshopsByUser(userId, offsetFilter).ConfigureAwait(false);

        if (favorites.TotalAmount == 0)
        {
            return NoContent();
        }

        return Ok(favorites);
    }

    /// <summary>
    /// Add a new Favorite to the database.
    /// </summary>
    /// <param name="dto">Favorite entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.FavoriteAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create(FavoriteDto dto)
    {
        var favorite = await service.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = favorite.Id, },
            favorite);
    }

    /// <summary>
    /// Update info about a Favorite in the database.
    /// </summary>
    /// <param name="dto">Favorite to update.</param>
    /// <returns>Favorite.</returns>
    [HasPermission(Permissions.FavoriteEdit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(FavoriteDto dto)
    {
        return Ok(await service.Update(dto).ConfigureAwait(false));
    }

    /// <summary>
    /// Delete a specific Favorite from the database.
    /// </summary>
    /// <param name="id">Favorite id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.FavoriteRemove)]
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