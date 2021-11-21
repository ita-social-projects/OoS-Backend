using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Pictures;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Pictures;

namespace OutOfSchool.WebApi.Services.Pictures
{
    public interface IPictureService
    {
        Task<PictureStorageModel> GetByIdAsync(Guid pictureId);

        Task<PictureOperationResult> UploadWorkshopPicture(Guid workshopId, PictureStorageModel pictureModel);
    }
}
