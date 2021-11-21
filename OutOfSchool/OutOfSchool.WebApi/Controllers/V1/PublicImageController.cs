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
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PublicImageController : ControllerBase
    {
        private readonly IImageService pictureService;

        public PublicImageController(IImageService pictureService)
        {
            this.pictureService = pictureService;
        }

        /// <summary>
        /// Gets <see cref="FileStreamResult"/> for a given pictureId.
        /// </summary>
        /// <param name="pictureId">This is the image id.</param>
        /// <returns>The result of <see cref="FileStreamResult"/>.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{pictureId}")]
        public async Task<IActionResult> GetByIdAsync(Guid pictureId)
        {
            var pictureData = await pictureService.GetByIdAsync(pictureId).ConfigureAwait(false);

            return new FileStreamResult(pictureData.ContentStream, pictureData.ContentType);
        }
    }
}
