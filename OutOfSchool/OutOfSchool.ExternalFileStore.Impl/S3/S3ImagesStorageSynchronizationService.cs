using Microsoft.Extensions.Logging;
using Minio.DataModel.Args;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.S3;

public class S3ImagesStorageSynchronizationService : ObjectImagesStorageSynchronizationService
{
    public S3ImagesStorageSynchronizationService(
        ILogger<S3ImagesStorageSynchronizationService> logger,
        IObjectImageStorage imagesStorage,
        IObjectImagesSyncDataRepository objectImagesSyncDataRepository) : base(logger, imagesStorage,
        objectImagesSyncDataRepository)
    {
    }

    protected override IAsyncEnumerable<StorageObject> GetListsOfObjects()
    {
        var options = new ListObjectsArgs().WithRecursive(true);
        return imagesStorage.ListObjectsAsync(options: options);
    }
}