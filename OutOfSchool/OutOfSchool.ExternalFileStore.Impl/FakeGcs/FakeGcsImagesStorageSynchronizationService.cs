using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.FakeGcs;

/// <summary>
/// Represents a gcp image files sync storage. It's used to synchronize FAKE gcp files with a database.
/// </summary>
public class FakeGcsImagesStorageSynchronizationService : ObjectImagesStorageSynchronizationService
{
    private const string ListObjectOptionsFields = "items(name,timeCreated),nextPageToken";


    public FakeGcsImagesStorageSynchronizationService(
        ILogger<FakeGcsImagesStorageSynchronizationService> logger,
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