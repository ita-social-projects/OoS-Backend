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
        public Task<ImageChangingResult> ChangeImagesAsync(WorkshopUpdateDto dto);
    }
}
