using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Configuration;

namespace OutOfSchool.Services.Repository
{
    public class PictureStorageMongo : IPictureStorage
    {
        private readonly MongoRepositoryConfiguration storageConfiguration;
        private readonly IGridFSBucket greedFSBucket;

        public PictureStorageMongo(IOptions<MongoRepositoryConfiguration> storageConfiguration)
        {
            this.storageConfiguration = storageConfiguration?.Value
                ?? throw new ArgumentNullException(nameof(storageConfiguration));

            this.greedFSBucket = GetGreedFSBucket();
        }

        /// <summary>
        /// Delete picture by it`s Id.
        /// </summary>
        /// <param name="objectId">Picture id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeletePicture(ObjectId objectId, CancellationToken cancellationToken)
        {
            try
            {
                await greedFSBucket.DeleteAsync(objectId, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new PictureStorageException(ex);
            }
        }

        /// <summary>
        /// Returns picture by picture Id.
        /// </summary>
        /// <param name="pictureId">Picture id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Stream> GetPictureByIdAsync(string pictureId)
            // Note: await should be here bacause of type conversion: Task<GridFSDownloadStream> cannot be automaticaly cast to the Task<Stream> ...
            => await greedFSBucket.OpenDownloadStreamAsync(new ObjectId(pictureId));

        /// <summary>
        /// Returns picture Id.
        /// </summary>
        /// <param name="contentStream">Content stream.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<string> UploadPicture(Stream contentStream, CancellationToken cancellationToken)
        {
            try
            {
                contentStream.Position = uint.MinValue;

                var objectId = await greedFSBucket.UploadFromStreamAsync(Guid.NewGuid().ToString(), contentStream, cancellationToken: cancellationToken).ConfigureAwait(false);

                return objectId.ToString();
            }
            catch (Exception ex)
            {
                throw new PictureStorageException(ex);
            }
        }

        private IGridFSBucket GetGreedFSBucket()
        {
            var client = new MongoClient(storageConfiguration.ConnectionString);
            var database = client.GetDatabase(storageConfiguration.PicturesTableName);

            return new GridFSBucket(database);
        }
    }
}
