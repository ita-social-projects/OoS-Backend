using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.PhotoStorage;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
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
        }

        /// <summary>
        /// Get files by it's keys.
        /// </summary>
        /// <param name="entityId">Id of the some entity.</param>
        /// <param name="entityType">Type of the some entity.</param>
        /// <returns>List of photos.</returns>
        [HttpGet("{entityId}/{entityType}")]
        [Authorize(Roles = "parent,provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFile(long entityId, EntityType entityType)
        {
            this.ValidateId(entityId, localizer);

            var bytes = await photoService.GetFile(entityId, entityType).ConfigureAwait(false);

            return File(bytes, MimeTypeMap.CurentContentType);
        }

        /// <summary>
        /// Get paths of the files by it's keys.
        /// </summary>
        /// <param name="entityId">Id of the some entity.</param>
        /// <param name="entityType">Type of the some entity.</param>
        /// <returns>List of paths.</returns>
        [HttpGet("{entityId}/{entityType}")]
        [Authorize(Roles = "parent,provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilesPaths(long entityId, EntityType entityType)
        {
            this.ValidateId(entityId, localizer);

            return Ok(await photoService.GetFilesPaths(entityId, entityType).ConfigureAwait(false));
        }

        /// <summary>
        /// Get path of the file by it's keys.
        /// </summary>
        /// <param name="entityId">Id of the some entity.</param>
        /// <param name="entityType">Type of the some entity.</param>
        /// <returns>Path of file.</returns>
        [HttpGet("{entityId}/{entityType}")]
        [Authorize(Roles = "parent,provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilePath(long entityId, EntityType entityType)
        {
            this.ValidateId(entityId, localizer);

            return Ok(await photoService.GetFilePath(entityId, entityType).ConfigureAwait(false));
        }

        /// <summary>
        /// Create new photo.
        /// </summary>
        /// <param name="photoInfoJson">Json string of the Photo entity.</param>
        /// <param name="photo">File.</param>
        /// <returns>Created photo info.</returns>
        [HttpPost]
        [Authorize(Roles = "parent,provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFile([FromForm] string photoInfoJson, [FromForm] IFormFile photo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (photoInfoJson is null)
            {
                return BadRequest("Json string of the Photo entity is null!");
            }

            if (photo is null)
            {
                return BadRequest("Photo is null!");
            }

            var photoInfo = JsonConvert.DeserializeObject<PhotoDto>(photoInfoJson);

            if (photoInfo is null)
            {
                return BadRequest("Photo Info is null!");
            }

            var fileName = $"{photoInfo.EntityId}_{photoInfo.EntityType}{Path.GetExtension(photo.FileName)}";

            var result = await photoService.AddFile(photo, photoInfo, fileName).ConfigureAwait(false);

            return Ok(result);
        }

        /// <summary>
        /// Create new photos.
        /// </summary>
        /// <param name="photoInfoJson">Json string of the Photo entity.</param>
        /// <param name="photos">Files.</param>
        /// <returns>Created photos info.</returns>
        [HttpPost]
        [Authorize(Roles = "parent,provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFiles([FromForm] string photoInfoJson, [FromForm] IFormFileCollection photos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (photoInfoJson is null)
            {
                return BadRequest("Json string of the Photo entity is null!");
            }

            if (photos is null)
            {
                return BadRequest("Photos is null!");
            }

            var photoInfo = JsonConvert.DeserializeObject<PhotoDto>(photoInfoJson);

            if (photoInfo is null)
            {
                return BadRequest("Photo Info is null!");
            }

            var result = await photoService.AddFiles(photos, photoInfo).ConfigureAwait(false);

            return Ok(result);
        }

        /// <summary>
        /// Delete a specific Photo entity.
        /// </summary>
        /// <param name="filePath">Photo path.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "parent,provider,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFile(string filePath)
        {
            if (filePath is null)
            {
                return BadRequest("File Path can not be null!");
            }

            await photoService.DeleteFile(filePath).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Delete a range of Photo entities.
        /// </summary>
        /// <param name="filesPaths">Paths of the photos.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "parent,provider,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFiles(List<string> filesPaths)
        {
            if (filesPaths is null)
            {
                return BadRequest("Paths of the files can not be null!");
            }

            await photoService.DeleteFiles(filesPaths).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Update info about the Photo.
        /// </summary>
        /// <param name="photoInfoJson">Json string of the Photo entity.</param>
        /// <param name="photo">New file.</param>
        /// <returns>Updated Photo.</returns>
        [HttpPut]
        [Authorize(Roles = "parent,provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateFile([FromForm] string photoInfoJson, [FromForm] IFormFile photo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (photoInfoJson is null)
            {
                return BadRequest("Json string of the Photo entity is null!");
            }

            if (photo is null)
            {
                return BadRequest("Photo is null!");
            }

            var photoInfo = JsonConvert.DeserializeObject<PhotoDto>(photoInfoJson);

            if (photoInfo is null)
            {
                return BadRequest("Photo Info is null!");
            }

            var result = await photoService.UpdateFile(photoInfo, photo).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
