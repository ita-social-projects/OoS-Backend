using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Files;
using OutOfSchool.WebApi.Models;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace OutOfSchool.WebApi.Services.Gcp
{
    public class GcpImagesStorageSynchronizationService : IGcpStorageSynchronizationService
    {
        private const string ListObjectOptionsFields = "items(name,timeCreated),nextPageToken";

        // TODO: must be changed to approximately 10000 after testing
        private const int ListObjectOptionsPageSize = 25;

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
        }

        public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogDebug("Gcp storage synchronization was started at utc time: {Time}", DateTime.UtcNow);

                // gcp returns data by local time
                var dateTime = DateTime.Now.AddMinutes(-1);

                var listsOfObjects = await GetListsOfObjects(cancellationToken).ConfigureAwait(false);
                var tasks = new List<Task>();

                await foreach (var objects in listsOfObjects.WithCancellation(cancellationToken))
                {
                    if (objects.Items is null || objects.Items.Count == 0)
                    {
                        break;
                    }

                    Console.WriteLine(objects.NextPageToken);
                    var mappedObjects = objects.Items
                        .Where(x => x.TimeCreated < dateTime)
                        .Select(x => x.Name)
                        .ToHashSet();

                    tasks.Add(Task.Run(
                        async () =>
                        {
                            var notAttachedIds = await SynchronizeAll(mappedObjects, cancellationToken).ConfigureAwait(false);
                            await RemoveNotAttachedIds(notAttachedIds).ConfigureAwait(false);
                        }, cancellationToken));

                    if (tasks.Count % 10 == 0)
                    {
                        await Task.WhenAll(tasks).ConfigureAwait(false); // in order to decrease CPU and memory using, works by parts count of 10 tasks
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Gcp storage synchronization was interrupted by error at utc time: {Time}", DateTime.UtcNow);
            }
            finally
            {
                logger.LogDebug("Gcp storage synchronization was finished at utc time: {Time}", DateTime.UtcNow);
            }
        }

        private List<Func<IEnumerable<string>, Task<List<string>>>> GetAllSyncFunctions()
        {
            return new List<Func<IEnumerable<string>, Task<List<string>>>>
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

        private async Task<HashSet<string>> SynchronizeAll(
            HashSet<string> searchIds,
            CancellationToken cancellationToken = default)
        {
            var allSyncFunctions = GetAllSyncFunctions();

            foreach (var syncFunction in allSyncFunctions)
            {
                var syncResult = await syncFunction(searchIds).ConfigureAwait(false);
                if (syncResult != null)
                {
                    searchIds.ExceptWith(syncResult);
                }
            }

            return searchIds;
        }

        private async Task<IAsyncEnumerable<Objects>> GetListsOfObjects(CancellationToken cancellationToken = default)
        {
            var options = new ListObjectsOptions
            {
                Fields = ListObjectOptionsFields,
                PageSize = ListObjectOptionsPageSize,
            };

            return await imagesStorage.GetBulkListsOfObjectsAsync(options: options).ConfigureAwait(false);
        }

        private async Task RemoveNotAttachedIds(IEnumerable<string> idsToRemove)
        {
            foreach (var id in idsToRemove)
            {
                try
                {
                    await imagesStorage.DeleteAsync(id).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Image was not deleted because the storage had thrown exception");
                }
            }
        }
    }
}