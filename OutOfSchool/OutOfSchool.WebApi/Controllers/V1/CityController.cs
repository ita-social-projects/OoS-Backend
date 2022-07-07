using System.Collections.Generic;
using System.Linq;
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
public class CityController : ControllerBase
{
    private const int MinimumNameLength = 3;
    private readonly ICityService service;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CityController"/> class.
    /// </summary>
    /// <param name="service">Service for City model.</param>
    /// <param name="localizer">Localizer.</param>
    public CityController(ICityService service, IStringLocalizer<SharedResource> localizer)
    {
        this.service = service;
        this.localizer = localizer;
    }

    ///// <summary>
    ///// Get all Cities from the database.
    ///// </summary>
    ///// <returns>List of all Cities.</returns>
    //[AllowAnonymous]
    //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CityDto>))]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //[HttpGet]
    //public async Task<IActionResult> Get()
    //{
    //    var cities = await service.GetAll().ConfigureAwait(false);

    //    if (!cities.Any())
    //    {
    //        return NoContent();
    //    }

    //    return Ok(cities);
    //}

    /// <summary>
    /// Get City by it's id.
    /// </summary>
    /// <param name="id">City id.</param>
    /// <returns>City.</returns>
    [HasPermission(Permissions.ImpersonalDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CityDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        this.ValidateId(id, localizer);

        return Ok(await service.GetById(id).ConfigureAwait(false));
    }

    /// <summary>
    /// Get all Cities from the database that starts on the name.
    /// </summary>
    /// <param name="name">City name.</param>
    /// <returns>List of Cities that starts on name.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CityDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetByName([FromQuery] string name)
    {
        if (name?.Length < MinimumNameLength)
        {
            return BadRequest();
        }

        var cities = await service.GetByCityName(name).ConfigureAwait(false);

        if (!cities.Any())
        {
            return NoContent();
        }

        return Ok(cities);
    }

    /// <summary>
    /// Get the nearest city by the filter from the database.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns>The closest city by the filter.</returns>
    [Route("[action]")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CityDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetNearestByFilter([FromQuery] CityFilter filter)
    {
        var city = await service.GetNearestCityByFilter(filter).ConfigureAwait(false);

        if (city is null)
        {
            return NoContent();
        }

        return Ok(city);
    }

    /// <summary>
    /// Add a new City to the database.
    /// </summary>
    /// <param name="dto">City entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create(CityDto dto)
    {
        var city = await service.Create(dto).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = city.Id, },
            city);
    }

    /// <summary>
    /// Update info about a City in the database.
    /// </summary>
    /// <param name="dto">City to update.</param>
    /// <returns>City.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(CityDto dto)
    {
        return Ok(await service.Update(dto).ConfigureAwait(false));
    }

    /// <summary>
    /// Delete a specific City from the database.
    /// </summary>
    /// <param name="id">City id.</param>
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