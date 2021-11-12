using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller with operations to get popular workshops and categories.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
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
        /// <param name="city">City to look for.</param>
        /// <returns>List of popular directions.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DirectionStatistic>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDirections(int limit, [FromQuery] string city)
        {
            var newLimit = ValidateNumberOfEntries(limit);

            var popularDirections = await service
                .GetPopularDirections(newLimit, city)
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
        /// <param name="city">City to look for.</param>
        /// <returns>List of popular workshops.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorkshops(int limit, [FromQuery] string city)
        {
            int newLimit = ValidateNumberOfEntries(limit);

            var popularWorkshops = await service
                .GetPopularWorkshops(newLimit, city)
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
}
