using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using OutOfSchool.BusinessLogic.Common.Synchronization;
using OutOfSchool.Services.Repository.Files;

namespace OutOfSchool.BusinessLogic.Services.Gcp;

/// <summary>
/// Represents a gcp image files sync storage. It's used to synchronize gcp files with a database.
/// </summary>
public class GcpImagesStorageSynchronizationService : IGcpStorageSynchronizationService
{
    private const string ListObjectOptionsFields = "items(name,timeCreated),nextPageToken";

    // TODO: must be changed to approximately 500 after testing
    private const int ListObjectOptionsPageSize = 50;

    private readonly ILogger<GcpImagesStorageSynchronizationService> logger;
    private readonly IImageFilesStorage imagesStorage;
    private readonly IGcpImagesSyncDataRepository gcpImagesSyncDataRepository;

    public GcpImagesStorageSynchronizationService(
        ILogger<GcpImagesStorageSynchronizationService> logger,
        IImageFilesStorage imagesStorage,
        IGcpImagesSyncDataRepository gcpImagesSyncDataRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.imagesStorage = imagesStorage ?? throw new ArgumentNullException(nameof(imagesStorage));
        this.gcpImagesSyncDataRepository = gcpImagesSyncDataRepository ?? throw new ArgumentNullException(nameof(gcpImagesSyncDataRepository));

        GetAllSynchronizationFunctions = new List<Func<IEnumerable<string>, Task<List<string>>>>
        {
            // Entity images
            gcpImagesSyncDataRepository.GetIntersectWorkshopImagesIds,
            gcpImagesSyncDataRepository.GetIntersectProviderImagesIds,

            // Entity cover images
            gcpImagesSyncDataRepository.GetIntersectWorkshopCoverImagesIds,
            gcpImagesSyncDataRepository.GetIntersectTeacherCoverImagesIds,
            gcpImagesSyncDataRepository.GetIntersectProviderCoverImagesIds,
        };
    }

    private List<Func<IEnumerable<string>, Task<List<string>>>> GetAllSynchronizationFunctions { get; }

    /// <inheritdoc/>
    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Gcp storage synchronization was started");

            var dateTime = DateTime.UtcNow.AddMinutes(GapConstants.GcpImagesSynchronizationDateTimeAddMinutesGap);

            var listsOfObjects = GetListsOfObjects().ConfigureAwait(false);

            await foreach (var objects in listsOfObjects.WithCancellation(cancellationToken))
            {
                if (objects.Items is null || objects.Items.Count == 0)
                {
                    break;
                }

                var mappedObjects = objects.Items
                    .Where(x => x.TimeCreated != null && TimeZoneInfo.ConvertTimeToUtc(x.TimeCreated.Value) < dateTime)
                    .Select(x => x.Name)
                    .ToHashSet();

                var notAttachedIds = await SynchronizeAll(mappedObjects).ConfigureAwait(false);
                await RemoveNotAttachedIds(notAttachedIds).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Gcp storage synchronization was interrupted by error");
        }
        finally
        {
            logger.LogDebug("Gcp storage synchronization was finished");
        }
    }

    private async Task<HashSet<string>> SynchronizeAll(HashSet<string> searchIds)
    {
        foreach (var synchronizationFunction in GetAllSynchronizationFunctions)
        {
            var syncResult = await synchronizationFunction(searchIds).ConfigureAwait(false);
            if (syncResult != null)
            {
                searchIds.ExceptWith(syncResult);
            }
        }

        return searchIds;
    }

    private IAsyncEnumerable<Objects> GetListsOfObjects()
    {
        var options = new ListObjectsOptions
        {
            Fields = ListObjectOptionsFields,
            PageSize = ListObjectOptionsPageSize,
        };

        return imagesStorage.GetBulkListsOfObjectsAsync(options: options);
    }

    private async Task RemoveNotAttachedIds(IEnumerable<string> idsToRemove)
    {
        foreach (var id in idsToRemove)
        {
            try
            {
                await imagesStorage.DeleteAsync(id).ConfigureAwait(false);
                logger.LogDebug("Image with id = {ImageId} was successfully deleted by synchronizer", id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Image was not deleted by synchronizer because the storage had thrown exception. Maybe it's already removed by user");
            }
        }
    }
}