using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with operations to get popular workshops and categories.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/popular")]
[ApiController]
public class StatisticController : ControllerBase
{
    private readonly IStatisticService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticController"/> class.
    /// </summary>
    /// <param name="service">Service to get statistic.</param>
    public StatisticController(IStatisticService service)
    {
        this.service = service;
    }

    /// <summary>
    /// Get popular directions.
    /// </summary>
    /// <param name="limit">The number of entries.</param>
    /// <param name="catottgId">Codeficator's id.</param>
    /// <returns>List of popular directions.</returns>
    [HttpGet("directions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DirectionDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    public async Task<IActionResult> GetDirections(int limit, [FromQuery] long catottgId)
    {
        var newLimit = ValidateNumberOfEntries(limit);

        var popularDirections = await service
            .GetPopularDirections(newLimit, catottgId)
            .ConfigureAwait(false);

        if (!popularDirections.Any())
        {
            return NoContent();
        }

        return Ok(popularDirections);
    }

    /// <summary>
    /// Get popular workshops.
    /// </summary>
    /// <param name="limit">The number of entries.</param>
    /// <param name="catottgId">Codeficator's id.</param>
    /// <returns>List of popular workshops.</returns>
    [HttpGet("workshops")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    public async Task<IActionResult> GetWorkshops(int limit, [FromQuery] long catottgId)
    {
        int newLimit = ValidateNumberOfEntries(limit);

        var popularWorkshops = await service
            .GetPopularWorkshops(newLimit, catottgId)
            .ConfigureAwait(false);

        if (!popularWorkshops.Any())
        {
            return NoContent();
        }

        return Ok(popularWorkshops);
    }

    private static int ValidateNumberOfEntries(int limit)
    {
        return limit < 3 ? 3 : (limit > 10 ? 10 : limit);
    }
}