using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with operations to get popular workshops and categories.
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]

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
        /// Get popular categories.
        /// </summary>
        /// <param name="limit">The number of entries.</param>
        /// <returns>List of popular categories.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryStatistic>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories(int limit)
        {
            var newLimit = ValidateNumberOfEntries(limit);

            var popularCategories = await service.GetPopularCategoriesFinal(newLimit).ConfigureAwait(false);

            if (!popularCategories.Any())
            {
                return NoContent();
            }

            return Ok(popularCategories);
        }

        /// <summary>
        /// Get popular workshops.
        /// </summary>
        /// <param name="limit">The number of entries.</param>
        /// <returns>List of popular workshops.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorkshops(int limit)
        {
            int newLimit = ValidateNumberOfEntries(limit);

            var popularWorkshops = await service.GetPopularWorkshops(newLimit).ConfigureAwait(false);

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
