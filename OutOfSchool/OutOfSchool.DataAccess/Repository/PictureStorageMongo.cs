using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using OutOfSchool.Services.Configuration;

namespace OutOfSchool.Services.Repository
{
    public class PictureStorageMongo : IPictureStorage
    {
        private readonly MongoRepositoryConfiguration storageConfiguration;

        public PictureStorageMongo(IOptions<MongoRepositoryConfiguration> storageConfiguration)
        {
            this.storageConfiguration = storageConfiguration?.Value
                ?? throw new ArgumentNullException(nameof(storageConfiguration));
        }

        private IGridFSBucket GetGreedFSBucket()
        {
            // TODO: check performance
            var client = new MongoClient(storageConfiguration.ConnectionString);
            var database = client.GetDatabase(storageConfiguration.PicturesTableName);

            return new GridFSBucket(database);
        }

        public async Task<Stream> GetPictureByIdAsync(string pictureId)
            // Note: await should be here bacause of type conversion: Task<GridFSDownloadStream> cannot be automaticaly cast to the Task<Stream> ...
            => await GetGreedFSBucket().OpenDownloadStreamAsync(new ObjectId(pictureId));
    }
}
