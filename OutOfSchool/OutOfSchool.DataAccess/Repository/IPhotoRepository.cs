using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IPhotoRepository : IEntityRepository<Photo>
    {
        void CreateUpdatePhoto(byte[] photo, string filePath);

        void DeletePhoto(string filePath);
    }
}
