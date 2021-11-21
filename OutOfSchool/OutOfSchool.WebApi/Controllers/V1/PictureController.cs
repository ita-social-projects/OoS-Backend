using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Pictures;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class PictureController : ControllerBase
    {
        private readonly IPictureService pictureService;
        private readonly IMapper mapper;

        public PictureController(IPictureService pictureService, IMapper mapper)
        {
            this.pictureService = pictureService;
            this.mapper = mapper;
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{pictureId}")]
        public async Task<IActionResult> GetWorkshopPictureAsync(Guid pictureId)
        {
            var pictureData = await pictureService.GetByIdAsync(pictureId).ConfigureAwait(false);

            return new FileStreamResult(pictureData.ContentStream, pictureData.ContentType);
        }
    }
}
