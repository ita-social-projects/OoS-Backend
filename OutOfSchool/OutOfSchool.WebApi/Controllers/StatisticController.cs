using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        public StatisticController(IStatisticService service, IStringLocalizer<SharedResource> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        [HttpGet("{number}")]
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

        [HttpGet("{number}")]
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
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(number), localizer["The number of entries cannot be less than 1."]);
            }
        }
    }
}
