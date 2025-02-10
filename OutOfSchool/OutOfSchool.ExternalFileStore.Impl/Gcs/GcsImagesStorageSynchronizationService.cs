using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.Gcs;

/// <summary>
/// Represents a gcp image files sync storage. It's used to synchronize gcp files with a database.
/// </summary>
public class GcsImagesStorageSynchronizationService : ObjectImagesStorageSynchronizationService
{
    private const string ListObjectOptionsFields = "items(name,timeCreated),nextPageToken";


    public GcsImagesStorageSynchronizationService(
        ILogger<GcsImagesStorageSynchronizationService> logger,
        IObjectImageStorage imagesStorage,
        IObjectImagesSyncDataRepository objectImagesSyncDataRepository) : base(logger, imagesStorage,
        objectImagesSyncDataRepository)
    {
    }

    protected override IAsyncEnumerable<StorageObject> GetListsOfObjects()
    {
        var options = new ListObjectsOptions
        {
            Fields = ListObjectOptionsFields,
            PageSize = ListObjectOptionsPageSize,
        };
        return imagesStorage.ListObjectsAsync(options: options);
    }
}