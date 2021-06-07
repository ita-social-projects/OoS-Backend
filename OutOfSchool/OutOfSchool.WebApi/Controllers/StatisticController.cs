using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService service;

        public StatisticController(IStatisticService service)
        {
            this.service = service;
        }

        [HttpGet("{number}")]
        public async Task<IActionResult> GetCategories(int number)
        {
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
            var popularWorkshops = await service.GetPopularWorkshops(number).ConfigureAwait(false);

            if (!popularWorkshops.Any())
            {
                return NoContent();
            }

            return Ok(popularWorkshops);
        }
    }
}
