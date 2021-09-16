using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Common.Exceptions;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        private readonly IPictureService pictureService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureController"/> class.
        /// </summary>
        /// <param name="pictureService">Service for picture model.</param>
        public PictureController(IPictureService pictureService)
        {
            this.pictureService = pictureService ?? throw new ArgumentNullException(nameof(pictureService));
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
            catch (WorkshopNotFoundException)
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
            catch (ProviderNotFoundException)
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
            catch (TeacherNotFoundException)
            {
                return NotFound(pictureId);
            }
            catch
            {
                return BadRequest("An error occured while receiving the picture.");
            }
        }
    }
}
