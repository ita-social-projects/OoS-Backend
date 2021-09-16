using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public interface IPictureService
    {
        Task<PictureStorageModel> GetPictureWorkshop(long workshopId, Guid pictureId);
    }
}
