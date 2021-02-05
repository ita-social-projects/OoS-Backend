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
    [Authorize]
    public class SectionController : ControllerBase
    {
        private ISectionService _sectionService;
        private readonly IMapper _mapper;

        public SectionController(ISectionService sectionService, IMapper mapper)
        {
            _sectionService = sectionService;
            _mapper = mapper;
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