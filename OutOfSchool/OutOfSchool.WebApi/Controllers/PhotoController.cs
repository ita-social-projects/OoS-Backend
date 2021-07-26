using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.PhotoStorage;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class PhotoController : ControllerBase
    {
        private readonly IPhotoStorage photoService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhotoController"/> class.
        /// </summary>
        /// <param name="photoService">Service for Photo model.</param>
        /// <param name="localizer">Localizer.</param>
        public PhotoController(IPhotoStorage photoService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.photoService = photoService;
            this.Path = $"{Directory.GetParent(Environment.CurrentDirectory).Parent.FullName}\\Photos";
        }

        private string Path { get; set; }

        [HttpGet("{entityId}/{entityType}")]
        public async Task<IActionResult> GetFiles(long entityId, EntityType entityType)
        {
            return Ok(await photoService.GetFiles(entityId, entityType).ConfigureAwait(false));
        }

        [HttpGet("{entityId}/{entityType}")]
        public async Task<IActionResult> GetFilesPaths(long entityId, EntityType entityType)
        {
            return Ok(await photoService.GetFilesPaths(entityId, entityType).ConfigureAwait(false));
        }

        [HttpGet("{entityId}/{entityType}")]
        public async Task<IActionResult> GetFilePath(long entityId, EntityType entityType)
        {
            return Ok(await photoService.GetFilePath(entityId, entityType).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<IActionResult> CreateFile(PhotoDto photoInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PhotoStorage.FilePath = $"{Path}\\{photoInfo.EntityType}";

            var fileName = $"{photoInfo.EntityId}_{photoInfo.EntityType}.{photoInfo.PhotoExtension.ToString().ToLower()}";

            var result = await photoService.AddFile(photoInfo, fileName).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFiles(List<PhotoDto> photosInfo)
        {
            if (photosInfo is null)
            {
                return BadRequest("Photos Info is null!");
            }

            PhotoStorage.FilePath = $"{Path}\\{photosInfo.FirstOrDefault().EntityType}";

            var result = await photoService.AddFiles(photosInfo).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFile(string filePath)
        {
            if (filePath is null)
            {
                return BadRequest("File Path can not be null!");
            }

            await photoService.DeleteFile(filePath).ConfigureAwait(false);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFiles(List<string> filesPaths)
        {
            if (filesPaths is null)
            {
                return BadRequest("Paths of the files can not be null!");
            }

            await photoService.DeleteFiles(filesPaths).ConfigureAwait(false);

            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateFile(PhotoDto photo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await photoService.UpdateFile(photo).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
