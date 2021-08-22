using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
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
        private readonly IContentTypeProvider contentTypeProvider;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly long maxSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhotoController"/> class.
        /// </summary>
        /// <param name="photoService">Service for Photo model.</param>
        /// <param name="contentTypeProvider">Provides a mapping between file extensions and MIME types.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="config">Config.</param>
        public PhotoController(IPhotoStorage photoService, IContentTypeProvider contentTypeProvider, IStringLocalizer<SharedResource> localizer, IConfiguration config)
        {
            this.localizer = localizer;
            this.photoService = photoService;
            this.contentTypeProvider = contentTypeProvider;
            this.maxSize = config.GetValue<long>("PhotoSettings:MaxSizeBytes");
        }

        /// <summary>
        /// Get file by it's name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Photo.</returns>
        [HttpGet]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var contentType = GetFormat(fileName);

            var stream = await photoService.GetFile(fileName).ConfigureAwait(false);

            return File(stream, contentType);
        }

        /// <summary>
        ///  Get names of the files of the provider entity by it's id.
        /// </summary>
        /// <param name="entityId">Id of the provider entity.</param>
        /// <returns>List of paths.</returns>
        [HttpGet("{entityId}")]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Provider(long entityId)
        {
            this.ValidateId(entityId, localizer);

            return Ok(await photoService.GetFilesNames(entityId, EntityType.Provider).ConfigureAwait(false));
        }

        /// <summary>
        /// Get names of the files of the workshop entity by it's id.
        /// </summary>
        /// <param name="entityId">Id of the workshop entity.</param>
        /// <returns>List of paths.</returns>
        [HttpGet("{entityId}")]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Workshop(long entityId)
        {
            this.ValidateId(entityId, localizer);

            return Ok(await photoService.GetFilesNames(entityId, EntityType.Workshop).ConfigureAwait(false));
        }

        /// <summary>
        /// Get file name of the teacher entity by it's id.
        /// </summary>
        /// <param name="entityId">Id of the teacher entity.</param>
        /// <returns>Path of file.</returns>
        [HttpGet("{entityId}")]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Teacher(long entityId)
        {
            this.ValidateId(entityId, localizer);

            return Ok(await photoService.GetFileName(entityId, EntityType.Teacher).ConfigureAwait(false));
        }

        /// <summary>
        /// Create new photo for teacher.
        /// </summary>
        /// <param name="photo">File.</param>
        /// <returns>Created photo info.</returns>
        [HttpPost]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PhotoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Teacher([FromForm] PhotoDto photo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (photo is null)
            {
                return BadRequest("Photo object is null.");
            }

            if (photo.File is null)
            {
                return BadRequest("Photo is null.");
            }

            if (!IsSizeValid(photo.File))
            {
                return BadRequest("The size of the photo invalid.");
            }

            if (!IsExtensionValid(photo.File))
            {
                return BadRequest("Extension of the photo invalid.");
            }

            var result = await photoService.AddFile(photo, EntityType.Teacher).ConfigureAwait(false);

            return Ok(result);
        }

        /// <summary>
        /// Create new photos for workshop.
        /// </summary>
        /// <param name="photos">Files.</param>
        /// <returns>Created photos info.</returns>
        [HttpPost]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PhotoDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Workshop([FromForm] PhotosDto photos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (photos is null)
            {
                return BadRequest("Photos object are null.");
            }

            foreach (var photo in photos.Files)
            {
                if (!IsSizeValid(photo))
                {
                    return BadRequest("The size of the photo invalid.");
                }

                if (!IsExtensionValid(photo))
                {
                    return BadRequest("Extension of the photo invalid.");
                }
            }

            var result = await photoService.AddFiles(photos, EntityType.Workshop).ConfigureAwait(false);

            return Ok(result);
        }

        /// <summary>
        /// Create new photos for provider.
        /// </summary>
        /// <param name="photos">Files.</param>
        /// <returns>Created photos info.</returns>
        [HttpPost]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PhotoDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Provider([FromForm] PhotosDto photos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (photos is null)
            {
                return BadRequest("Photos object are null.");
            }

            foreach (var photo in photos.Files)
            {
                if (!IsSizeValid(photo))
                {
                    return BadRequest("The size of the photo invalid.");
                }

                if (!IsExtensionValid(photo))
                {
                    return BadRequest("Extension of the photo invalid.");
                }
            }

            var result = await photoService.AddFiles(photos, EntityType.Provider).ConfigureAwait(false);

            return Ok(result);
        }

        /// <summary>
        /// Delete a specific Photo entity.
        /// </summary>
        /// <param name="fileName">Photo name.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            if (fileName is null)
            {
                return BadRequest("File Path can not be null.");
            }

            await photoService.DeleteFile(fileName).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Delete a range of Photo entities.
        /// </summary>
        /// <param name="filesNames">Names of the photos.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpDelete]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFiles(List<string> filesNames)
        {
            if (filesNames is null)
            {
                return BadRequest("Paths of the files can not be null.");
            }

            await photoService.DeleteFiles(filesNames).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Update info about the Photo.
        /// </summary>
        /// <param name="photo">New file.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPut]
        [Authorize(Roles = "parent")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateFile([FromForm] PhotoDto photo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (photo is null)
            {
                return BadRequest("Photo object is null.");
            }

            if (photo.File is null)
            {
                return BadRequest("Photo is null.");
            }

            if (!IsSizeValid(photo.File))
            {
                return BadRequest("The size of the photo invalid.");
            }

            if (!IsExtensionValid(photo.File))
            {
                return BadRequest("Extension of the photo invalid.");
            }

            await photoService.UpdateFile(photo).ConfigureAwait(false);

            return NoContent();
        }

        private bool IsSizeValid(IFormFile photo)
        {
            if (photo.Length > maxSize)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool IsExtensionValid(IFormFile photo)
        {
            var contentType = GetFormat(photo.FileName);

            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private string GetFormat(string fileName)
        {
            string contentType;

            if (!contentTypeProvider.TryGetContentType(fileName, out contentType))
            {
                contentType = string.Empty;
            }

            if (contentType is ImageTypeNames.Jpeg || contentType is ImageTypeNames.Png)
            {
                return contentType;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
