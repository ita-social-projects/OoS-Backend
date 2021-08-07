using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OutOfSchool.Services.Repository
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly string basePhotoPath;

        public PhotoRepository(IConfiguration config)
        {
            basePhotoPath = config.GetSection("PhotoSettings:BasePath").Value;
        }

        /// <summary>
        /// Create photo.
        /// </summary>
        /// <param name="photo">Photo as byte array.</param>
        /// <param name="fileName">Photo name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateUpdatePhoto(byte[] photo, string fileName)
        {
            var filePath = GenerateFilePath(fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await File.WriteAllBytesAsync(filePath, photo).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete photo by path.
        /// </summary>
        /// <param name="fileName">Photo name.</param>
        public void DeletePhoto(string fileName)
        {
            File.Delete(GenerateFilePath(fileName));
        }

        /// <summary>
        /// Returns photo by path.
        /// </summary>
        /// <param name="fileName">Photo name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<byte[]> GetPhoto(string fileName)
        {
            return await File.ReadAllBytesAsync(GenerateFilePath(fileName)).ConfigureAwait(false);
        }

        private string GenerateFilePath(string fileName)
        {
            return Path.Combine(basePhotoPath, fileName);
        }
    }
}
