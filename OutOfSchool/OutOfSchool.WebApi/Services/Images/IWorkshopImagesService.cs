using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IWorkshopImagesService : IImageInteractionService<Guid>
    {
        /// <summary>
        /// Removes unnecessary images, comparing given in ImageIds and adds new images from the ImageFiles.
        /// </summary>
        /// <param name="dto">The instance of <see cref="WorkshopUpdateDto"/>.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ImageChangingResult"/> of the operation.</returns>
        public Task<ImageChangingResult> ChangeImagesAsync(WorkshopUpdateDto dto);
    }
}
