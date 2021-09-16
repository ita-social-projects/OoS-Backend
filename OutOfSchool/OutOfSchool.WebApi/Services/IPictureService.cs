using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IPictureService
    {
        Task<PictureStorageModel> GetPictureWorkshop(long workshopId, Guid pictureId);

        Task<PictureStorageModel> GetPictureProvider(long providerId, Guid pictureId);

        Task<PictureStorageModel> GetPictureTeacher(long teacherId, Guid pictureId);
    }
}
