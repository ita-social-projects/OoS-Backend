using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using OutOfSchool.WebApi.Common.Exceptions;
using OutOfSchool.WebApi.Common.Utilities;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        private const string KeyToMaxPictureSize = "PictureSettings:MaxSizeBytes";

        private readonly IPictureService pictureService;
        private readonly IContentTypeProvider contentTypeProvider;
        private readonly long maxPictureSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureController"/> class.
        /// </summary>
        /// <param name="pictureService">Service for picture model.</param>
        /// <param name="contentTypeProvider">Content type provider.</param>
        /// <param name="config">Config.</param>
        public PictureController(IPictureService pictureService, IContentTypeProvider contentTypeProvider, IConfiguration config)
        {
            this.pictureService = pictureService ?? throw new ArgumentNullException(nameof(pictureService));
            this.contentTypeProvider = contentTypeProvider ?? throw new ArgumentNullException(nameof(contentTypeProvider));
            this.maxPictureSize = config.GetValue<long>(KeyToMaxPictureSize);
        }

        /// <summary>
        /// Get picture by it's id and workshop Id.
        /// </summary>
        /// <param name="workshopId">Id of the Workshop Entity.</param>
        /// <param name="pictureId">Id of the picture.</param>
        /// <returns>Workshop picture.</returns>
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("workshop/{workshopId}/picture/{pictureId}")]
        public async Task<IActionResult> Workshop(long workshopId, Guid pictureId)
        {
            try
            {
                var pictureData = await pictureService.GetPictureWorkshop(workshopId, pictureId).ConfigureAwait(false);

                return pictureData.ToFileResult();
            }
            catch (EntityNotFoundException)
            {
                return NotFound(pictureId);
            }
            catch
            {
                return BadRequest("An error occured while receiving the picture.");
            }
        }

        /// <summary>
        /// Get picture by it's id and provider Id.
        /// </summary>
        /// <param name="providerId">Id of the Provider Entity.</param>
        /// <param name="pictureId">Id of the picture.</param>
        /// <returns>Provider picture.</returns>
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("provider/{providerId}/picture/{pictureId}")]
        public async Task<IActionResult> Provider(long providerId, Guid pictureId)
        {
            try
            {
                var pictureData = await pictureService.GetPictureProvider(providerId, pictureId).ConfigureAwait(false);

                return pictureData.ToFileResult();
            }
            catch (EntityNotFoundException)
            {
                return NotFound(pictureId);
            }
            catch
            {
                return BadRequest("An error occured while receiving the picture.");
            }
        }

        /// <summary>
        /// Get picture by it's id and teacher Id.
        /// </summary>
        /// <param name="teacherId">Id of the Teacher Entity.</param>
        /// <param name="pictureId">Id of the picture.</param>
        /// <returns>Teacher picture.</returns>
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("teacher/{teacherId}/picture/{pictureId}")]
        public async Task<IActionResult> Teacher(long teacherId, Guid pictureId)
        {
            try
            {
                var pictureData = await pictureService.GetPictureTeacher(teacherId, pictureId).ConfigureAwait(false);

                return pictureData.ToFileResult();
            }
            catch (EntityNotFoundException)
            {
                return NotFound(pictureId);
            }
            catch
            {
                return BadRequest("An error occured while receiving the picture.");
            }
        }

        /// <summary>
        /// Uploads Teacher picture to data storage.
        /// </summary>
        /// <param name="teacherId">Id of the Teacher Entity.</param>
        /// <param name="picture">Picture.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Route("upload/teacher/{teacherId}")]
        public async Task<IActionResult> UploadPhotoTeacher(long teacherId, IFormFile picture)
        {
            try
            {
                if (teacherId <= 0)
                {
                    return BadRequest("TeacherId is invalid.");
                }

                if (picture is null)
                {
                    return BadRequest("Picture is missing.");
                }

                if (!IsSizeValid(picture))
                {
                    return BadRequest("The size of the picture invalid.");
                }

                if (!IsExtensionValid(picture))
                {
                    return BadRequest("Extension of the picture invalid.");
                }

                using (var fileStream = new MemoryStream())
                {
                    fileStream.Position = uint.MinValue;
                    await picture.CopyToAsync(fileStream, CancellationToken.None).ConfigureAwait(false);
                    await pictureService.UploadPictureTeacher(teacherId, fileStream, picture.ContentType).ConfigureAwait(false);
                }

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while uploading the picture.");
            }
        }

        /// <summary>
        /// Uploads Workshop picture to data storage.
        /// </summary>
        /// <param name="workshopId">Id of the Workshop Entity.</param>
        /// <param name="picture">Picture.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Route("upload/workshop/{workshopId}")]
        public async Task<IActionResult> UploadPhotoWorkshop(long workshopId, IFormFile picture)
        {
            try
            {
                if (workshopId <= 0)
                {
                    return BadRequest("WorkshopId is invalid.");
                }

                if (picture is null)
                {
                    return BadRequest("Picture is missing.");
                }

                if (!IsSizeValid(picture))
                {
                    return BadRequest("The size of the picture invalid.");
                }

                if (!IsExtensionValid(picture))
                {
                    return BadRequest("Extension of the picture invalid.");
                }

                using (var fileStream = new MemoryStream())
                {
                    fileStream.Position = uint.MinValue;
                    await picture.CopyToAsync(fileStream, CancellationToken.None).ConfigureAwait(false);
                    await pictureService.UploadPictureWorkshop(workshopId, fileStream, picture.ContentType).ConfigureAwait(false);
                }

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while uploading the picture.");
            }
        }

        /// <summary>
        /// Uploads Provider picture to data storage.
        /// </summary>
        /// <param name="providerId">Id of the Provider Entity.</param>
        /// <param name="picture">Picture.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Authorize(Roles = "provider")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Route("upload/provider/{providerId}")]
        public async Task<IActionResult> UploadPhotoProvider(long providerId, IFormFile picture)
        {
            try
            {
                if (providerId <= 0)
                {
                    return BadRequest("ProviderId is invalid.");
                }

                if (picture is null)
                {
                    return BadRequest("Picture is missing.");
                }

                if (!IsSizeValid(picture))
                {
                    return BadRequest("The size of the picture invalid.");
                }

                if (!IsExtensionValid(picture))
                {
                    return BadRequest("Extension of the picture invalid.");
                }

                using (var fileStream = new MemoryStream())
                {
                    fileStream.Position = uint.MinValue;
                    await picture.CopyToAsync(fileStream, CancellationToken.None).ConfigureAwait(false);
                    await pictureService.UploadPictureProvider(providerId, fileStream, picture.ContentType).ConfigureAwait(false);
                }

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while uploading the picture.");
            }
        }

        private bool IsSizeValid(IFormFile photo)
        {
            return photo.Length > maxPictureSize ? false : true;
        }

        private bool IsExtensionValid(IFormFile picture)
        {
            var contentType = GetFormat(picture.FileName);

            return string.IsNullOrEmpty(contentType) ? false : true;
        }

        private string GetFormat(string fileName)
        {
            string contentType;

            if (!contentTypeProvider.TryGetContentType(fileName, out contentType))
            {
                return string.Empty;
            }

            return contentType == string.Intern(PictureTypeNames.Jpeg) || contentType == string.Intern(PictureTypeNames.Png) ? contentType : string.Empty;
        }
    }
}
