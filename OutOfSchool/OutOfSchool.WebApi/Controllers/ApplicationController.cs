using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Services;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]

    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService service;
        private readonly IStringLocalizer localizer;

        public ApplicationController(IApplicationService service, IStringLocalizer localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var applications = await service.GetAll().ConfigureAwait(false);

            if (!applications.Any())
            {
                return NoContent();
            }

            return Ok(applications);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(long id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            return Ok(await service.GetById(id).ConfigureAwait(false));
        }

        [HttpGet]
        public async Task<IActionResult> GetByUserId(string id)
        {
            try
            {
                return Ok(await service.GetAllByUser(id).ConfigureAwait(false));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByWorkshopId(long id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            try
            {
                return Ok(await service.GetAllByWorkshop(id).ConfigureAwait(false));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationDTO applicationDto)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                applicationDto.UserId = User.FindFirst("sub")?.Value;

                var application = await service.Create(applicationDto).ConfigureAwait(false);

                return CreatedAtAction(
                     nameof(GetById),
                     new { id = application.Id, },
                     application);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(ApplicationDTO applicationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(applicationDto).ConfigureAwait(false));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(long id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            await service.Delete(id).ConfigureAwait(false);

            return NoContent();
        }
    }
}
