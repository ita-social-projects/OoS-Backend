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

        // TODO: add try/catch
        public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
        {
            var dateTimeNowUtc = DateTime.Now;
            var jokeData = dateTimeNowUtc.AddDays(-1);

            var listsOfObjects = await GetListsOfObjects(cancellationToken).ConfigureAwait(false);

            var deleteIds = new List<string>();

            // TODO: add TPL usage for every pack of objects
            await foreach (var objects in listsOfObjects.WithCancellation(cancellationToken))
            {
                Debug.WriteLine(objects.NextPageToken);
                var mappedObjects = objects.Items
                    .Where(x => x.TimeCreated <= jokeData)
                    .Select(x => x.Name)
                    .ToHashSet();

                var notAttachedIds = await SynchronizeAll(mappedObjects, cancellationToken).ConfigureAwait(false);
                deleteIds.AddRange(notAttachedIds);
                // TODO: uncomment this
                // await RemoveNotAttachedIds(notAttachedIds).ConfigureAwait(false);

                Console.WriteLine($"{objects.Items.Count} : {mappedObjects.Count()}");
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
                Fields = "items(name,timeCreated),nextPageToken",
                PageSize = 50,
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
                catch (Exception e)
                {
                    logger.LogError(e, "Image was not deleted because the storage had thrown exception");
                }
            }
        }
    }
}