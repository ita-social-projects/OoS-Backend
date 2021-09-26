using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace OutOfSchool.Services.Repository
{
    public interface IPictureStorage
    {
        Task<Stream> GetPictureByIdAsync(string pictureId);

        Task<string> UploadPicture(Stream contentStream, CancellationToken cancellationToken);

        Task DeletePicture(ObjectId objectId, CancellationToken cancellationToken);
    }
}
