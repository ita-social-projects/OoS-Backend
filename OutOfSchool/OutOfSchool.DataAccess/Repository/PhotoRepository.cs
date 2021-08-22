﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OutOfSchool.Services.Repository
{
    public class PhotoRepository : IPhotoRepository
    {
        private const string Key = "PhotoSettings:BasePath";
        private readonly string basePhotoPath;

        public PhotoRepository(IConfiguration config)
        {
            basePhotoPath = config.GetSection(Key).Value;
        }

        /// <summary>
        /// Create photo.
        /// </summary>
        /// <param name="photo">Photo as byte array.</param>
        /// <param name="fileName">Photo name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CreateUpdatePhotoAsync(byte[] photo, string fileName)
        {
            var filePath = GenerateFilePath(fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            return File.WriteAllBytesAsync(filePath, photo);
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
        public async Task<Stream> GetPhotoAsync(string fileName)
        {
            return await Task.Run(() =>
            {
                return new FileStream(GenerateFilePath(fileName), FileMode.Open);
            });
        }

        private string GenerateFilePath(string fileName)
        {
            return Path.Combine(basePhotoPath, fileName);
        }
    }
}
