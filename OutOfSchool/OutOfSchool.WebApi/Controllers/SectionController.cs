using System;
using System.Threading.Tasks;
using System.Web.WebPages.Html;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes  = "Bearer")]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpPost]
        public async Task<ActionResult<Section>> Create([FromBody] SectionDto section)
        {
            if (ModelState.IsValid)
            {
                return await _sectionService.CreateAsync(section);
            }

            return BadRequest(ModelState);
        }
    }
}