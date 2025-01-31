using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.Extensions;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore;

public abstract class ObjectImagesStorageSynchronizationService : IObjectStorageSynchronizationService
{
    // TODO: must be changed to approximately 500 after testing
    protected const int ListObjectOptionsPageSize = 50;

    private readonly ILogger<ObjectImagesStorageSynchronizationService> logger;
    protected readonly IObjectImageStorage imagesStorage;

    protected ObjectImagesStorageSynchronizationService(
        ILogger<ObjectImagesStorageSynchronizationService> logger,
        IObjectImageStorage imagesStorage,
        IObjectImagesSyncDataRepository objectImagesSyncDataRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.imagesStorage = imagesStorage ?? throw new ArgumentNullException(nameof(imagesStorage));

        GetAllSynchronizationFunctions =
        [
            // Entity images
            objectImagesSyncDataRepository.GetIntersectWorkshopImagesIds,
            objectImagesSyncDataRepository.GetIntersectProviderImagesIds,

            // Entity cover images
            objectImagesSyncDataRepository.GetIntersectWorkshopCoverImagesIds,
            objectImagesSyncDataRepository.GetIntersectTeacherCoverImagesIds,
            objectImagesSyncDataRepository.GetIntersectProviderCoverImagesIds
        ];
    }

    protected abstract IAsyncEnumerable<StorageObject> GetListsOfObjects();

    private List<Func<IEnumerable<string>, Task<List<string>>>> GetAllSynchronizationFunctions { get; }

    /// <inheritdoc/>
    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Gcp storage synchronization was started");

            var dateTime = DateTimeOffset.UtcNow.AddMinutes(GapConstants.GcpImagesSynchronizationDateTimeAddMinutesGap);

            await foreach (var objects in this.GetListsOfObjects()
                               .Where(x => x.CreatedAt != null && x.CreatedAt.Value.ToUniversalTime() < dateTime)
                               .Select(x => x.Name)
                               .BatchAsync(ListObjectOptionsPageSize, cancellationToken)
                               .ConfigureAwait(false))
            {
                var mappedObjects = objects.ToHashSet();

                var notAttachedIds = await this.SynchronizeAll(mappedObjects).ConfigureAwait(false);
                await this.RemoveNotAttachedIds(notAttachedIds).ConfigureAwait(false);
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
                logger.LogError(ex,
                    "Image was not deleted by synchronizer because the storage had thrown exception. Maybe it's already removed by user");
            }
        }
    }
}