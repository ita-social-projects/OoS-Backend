using System;
using System.Threading.Tasks;
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

        public PictureController(IPictureService pictureService)
        {
            this.pictureService = pictureService ?? throw new ArgumentNullException(nameof(pictureService));
        }

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
                return BadRequest();
            }
        }

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
                return BadRequest();
            }
        }

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
                return BadRequest();
            }
        }
    }
}
