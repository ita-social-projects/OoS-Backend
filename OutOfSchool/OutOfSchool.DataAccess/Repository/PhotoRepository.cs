using System.IO;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class PhotoRepository : EntityRepository<Photo>, IPhotoRepository
    {
        private readonly OutOfSchoolDbContext db;

        public PhotoRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        /// <summary>
        /// Create photo.
        /// </summary>
        /// <param name="photo">Photo as byte array.</param>
        /// <param name="filePath">Photo path.</param>
        public void CreateUpdatePhoto(byte[] photo, string filePath)
        {
            File.WriteAllBytesAsync(filePath, photo);
        }

        /// <summary>
        /// Delete photo by path.
        /// </summary>
        /// <param name="filePath">Photo path.</param>
        public void DeletePhoto(string filePath)
        {
            File.Delete(filePath);
        }
    }
}
