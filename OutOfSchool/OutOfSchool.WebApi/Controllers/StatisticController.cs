using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticController"/> class.
        /// </summary>
        /// <param name="service">Service to get statistic.</param>
        /// <param name="localizer">Localizer.</param>
        public StatisticController(IStatisticService service, IStringLocalizer<SharedResource> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get popular categories.
        /// </summary>
        /// <param name="number">The number of entries.</param>
        /// <returns>List of popular categories.</returns>
        [HttpGet("{number}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories(int number)
        {
            ValidateNumberOfEntries(number);

            var popularCategories = await service.GetPopularCategories(number).ConfigureAwait(false);

            if (!popularCategories.Any())
            {
                return NoContent();
            }

            return Ok(popularCategories);
        }

        /// <summary>
        /// Get popular workshops.
        /// </summary>
        /// <param name="number">The number of entries.</param>
        /// <returns>List of popular workshops.</returns>
        [HttpGet("{number}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorkshops(int number)
        {
            ValidateNumberOfEntries(number);

            var popularWorkshops = await service.GetPopularWorkshops(number).ConfigureAwait(false);

            if (!popularWorkshops.Any())
            {
                return NoContent();
            }

            return Ok(popularWorkshops);
        }

        private void ValidateNumberOfEntries(int number)
        {
            if (number < 3 || number > 10)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(number), 
                    localizer["The number of entries must be in range from 3 to 10."]);
            }
        }
    }
}
