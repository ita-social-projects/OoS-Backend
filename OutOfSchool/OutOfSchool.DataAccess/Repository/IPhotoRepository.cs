using System.IO;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public interface IPhotoRepository
    {
        Task CreateUpdatePhotoAsync(byte[] photo, string fileName);

        Task<Stream> GetPhotoAsync(string fileName);

        void DeletePhoto(string fileName);
    }
}
