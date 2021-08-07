using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public interface IPhotoRepository
    {
        Task CreateUpdatePhoto(byte[] photo, string fileName);

        Task<byte[]> GetPhoto(string fileName);

        void DeletePhoto(string fileName);
    }
}
