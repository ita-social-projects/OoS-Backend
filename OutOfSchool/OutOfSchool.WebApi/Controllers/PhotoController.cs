using System;
using System.Collections.Generic;
using System.IO;
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
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFile(string fileName)
        {
            try
            {
                var contentType = GetFormat(fileName);

                if (string.IsNullOrEmpty(contentType))
                {
                    return BadRequest("Invalid file format");
                }

                var stream = await photoService.GetFile(fileName).ConfigureAwait(false);

                return File(stream, contentType);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is InvalidOperationException)
            {
                return BadRequest("An error occured while receiving the photo.");
            }
        }

        /// <summary>
        ///  Get names of the files of the provider entity by it's id.
        /// </summary>
        /// <param name="entityId">Id of the provider entity.</param>
        /// <returns>List of paths.</returns>
        [HttpGet("{entityId}")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Task<List<string>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Provider(long entityId)
        {
            try
            {
                this.ValidateId(entityId, localizer);

                return Ok(await photoService.GetFilesNames(entityId, EntityType.Provider).ConfigureAwait(false));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                return BadRequest("An error occurred while getting names of the files.");
            }
        }

        /// <summary>
        /// Get names of the files of the workshop entity by it's id.
        /// </summary>
        /// <param name="entityId">Id of the workshop entity.</param>
        /// <returns>List of paths.</returns>
        [HttpGet("{entityId}")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK,  Type = typeof(Task<List<string>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Workshop(long entityId)
        {
            try
            {
                this.ValidateId(entityId, localizer);

                return Ok(await photoService.GetFilesNames(entityId, EntityType.Workshop).ConfigureAwait(false));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                return BadRequest("An error occurred while getting names of the files.");
            }
        }

        /// <summary>
        /// Get file name of the teacher entity by it's id.
        /// </summary>
        /// <param name="entityId">Id of the teacher entity.</param>
        /// <returns>Path of file.</returns>
        [HttpGet("{entityId}")]
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Task<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Teacher(long entityId)
        {
            try
            {
                this.ValidateId(entityId, localizer);

                return Ok(await photoService.GetFileName(entityId, EntityType.Teacher).ConfigureAwait(false));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is InvalidOperationException)
            {
                return BadRequest("An error occurred while getting name of the file.");
            }
        }

        /// <summary>
        /// Create new photo for teacher.
        /// </summary>
        /// <param name="photo">File.</param>
        /// <returns>Created photo info.</returns>
        [HttpPost]
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
                return BadRequest("The information about the photo is missing.");
            }

            if (photo.File is null)
            {
                return BadRequest("Photo is missing.");
            }

            if (!IsSizeValid(photo.File))
            {
                return BadRequest("The size of the photo invalid.");
            }

            if (!IsExtensionValid(photo.File))
            {
                return BadRequest("Extension of the photo invalid.");
            }

            try
            {
                var result = await photoService.AddFile(photo, EntityType.Teacher).ConfigureAwait(false);

                return Ok(result);
            }
            catch (IOException)
            {
                return BadRequest("An error occurred while creating the photo.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Create new photos for workshop.
        /// </summary>
        /// <param name="photos">Files.</param>
        /// <returns>Created photos info.</returns>
        [HttpPost]
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
                return BadRequest("The information about photos is missing.");
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

            try
            {
                var result = await photoService.AddFiles(photos, EntityType.Workshop).ConfigureAwait(false);

                return Ok(result);
            }
            catch (IOException)
            {
                return BadRequest("An error occurred while creating photos.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Create new photos for provider.
        /// </summary>
        /// <param name="photos">Files.</param>
        /// <returns>Created photos info.</returns>
        [HttpPost]
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
                return BadRequest("The information about photos is missing.");
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

            try
            {
                var result = await photoService.AddFiles(photos, EntityType.Provider).ConfigureAwait(false);

                return Ok(result);
            }
            catch (IOException)
            {
                return BadRequest("An error occurred while creating photos.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Delete a specific Photo entity.
        /// </summary>
        /// <param name="fileName">Photo name.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpDelete]
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
                return BadRequest("File name is missing.");
            }

            try
            {
                await photoService.DeleteFile(fileName).ConfigureAwait(false);

                return NoContent();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is InvalidOperationException)
            {
                return BadRequest("An error occurred while deleting the photo.");
            }
        }

        /// <summary>
        /// Delete a range of Photo entities.
        /// </summary>
        /// <param name="filesNames">Names of the photos.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpDelete]
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
                return BadRequest("File names are missing.");
            }

            try
            {
                await photoService.DeleteFiles(filesNames).ConfigureAwait(false);

                return NoContent();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is InvalidOperationException)
            {
                return BadRequest("An error occurred while deleting photos.");
            }
        }

        /// <summary>
        /// Update info about the Photo.
        /// </summary>
        /// <param name="photo">New file.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPut]
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
                return BadRequest("The information about the photo is missing.");
            }

            if (photo.File is null)
            {
                return BadRequest("Photo is missing.");
            }

            if (!IsSizeValid(photo.File))
            {
                return BadRequest("The size of the photo invalid.");
            }

            if (!IsExtensionValid(photo.File))
            {
                return BadRequest("Extension of the photo invalid.");
            }

            try
            {
                await photoService.UpdateFile(photo).ConfigureAwait(false);

                return NoContent();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is InvalidOperationException)
            {
                return BadRequest("An error occurred while updating the photo.");
            }
        }

        private bool IsSizeValid(IFormFile photo)
        {
            return photo.Length > maxSize ? false : true;
        }

        private bool IsExtensionValid(IFormFile photo)
        {
            var contentType = GetFormat(photo.FileName);

            return string.IsNullOrEmpty(contentType) ? false : true;
        }

        private string GetFormat(string fileName)
        {
            string contentType;

            if (!contentTypeProvider.TryGetContentType(fileName, out contentType))
            {
                return string.Empty;
            }

            return contentType == string.Intern(ImageTypeNames.Jpeg) || contentType == string.Intern(ImageTypeNames.Png) ? contentType : string.Empty;
        }
    }
}
