using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private readonly IWorkshopRepository workshopRepository;
        private readonly ISensitiveEntityRepository<Teacher> teacherRepository;
        private readonly IProviderRepository providerRepository;

        public GcpImagesStorageSynchronizationService(
            ILogger<GcpImagesStorageSynchronizationService> logger,
            IWorkshopRepository workshopRepository,
            ISensitiveEntityRepository<Teacher> teacherRepository,
            IProviderRepository providerRepository,
            IImageFilesStorage imagesStorage)
        {
            this.logger = logger;
            this.imagesStorage = imagesStorage;
            this.workshopRepository = workshopRepository;
            this.teacherRepository = teacherRepository;
            this.providerRepository = providerRepository;
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

                // TODO: uncomment this
                // await RemoveNotAttachedIds(notAttachedIds).ConfigureAwait(false);

                Console.WriteLine($"{objects.Items.Count} : {mappedObjects.Count()}");
            }
        }

        private static async Task<List<string>> SynchronizeEntityCoverImages<TRepository, TEntity, TKey>(TRepository repository, IEnumerable<string> searchIds)
            where TRepository : IEntityRepositoryBase<TKey, TEntity>
            where TEntity : class, IKeyedEntity<TKey>, IImageDependentEntity<TEntity>, new()
        {
            return await repository
                .GetByFilterNoTracking(x => searchIds.Contains(x.CoverImageId))
                .Select(x => x.CoverImageId)
                .ToListAsync().ConfigureAwait(false);
        }

        private static async Task<List<string>> SynchronizeEntityImages<TRepository, TEntity, TKey>(TRepository repository, IEnumerable<string> searchIds)
            where TRepository : IEntityRepositoryBase<TKey, TEntity>
            where TEntity : class, IKeyedEntity<TKey>, IImageDependentEntity<TEntity>, new()
        {
            return await repository
                .GetByFilterNoTracking(x => !x.Images
                    .Select(i => i.ExternalStorageId)
                    .Intersect(searchIds).Any())
                .SelectMany(x => x.Images.Select(i => i.ExternalStorageId))
                .ToListAsync().ConfigureAwait(false);
        }

        private List<Func<IEnumerable<string>, Task<List<string>>>> GetAllSyncFunctions()
        {
            return new List<Func<IEnumerable<string>, Task<List<string>>>>
            {
                SynchronizeWorkshopImages,
                SynchronizeProviderImages,
                SynchronizeWorkshopCoverImages,
                SynchronizeTeacherCoverImages,
                SynchronizeProviderCoverImages,
            };
        }

        private async Task<HashSet<string>> SynchronizeAll(HashSet<string> searchIds, CancellationToken cancellationToken = default)
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

        private async Task<List<string>> SynchronizeWorkshopCoverImages(IEnumerable<string> searchIds)
            => await SynchronizeEntityCoverImages<IWorkshopRepository, Workshop, Guid>(workshopRepository, searchIds).ConfigureAwait(false);

        private async Task<List<string>> SynchronizeTeacherCoverImages(IEnumerable<string> searchIds)
            => await SynchronizeEntityCoverImages<ISensitiveEntityRepository<Teacher>, Teacher, Guid>(teacherRepository, searchIds).ConfigureAwait(false);

        private async Task<List<string>> SynchronizeProviderCoverImages(IEnumerable<string> searchIds)
            => await SynchronizeEntityCoverImages<IProviderRepository, Provider, Guid>(providerRepository, searchIds).ConfigureAwait(false);

        private async Task<List<string>> SynchronizeWorkshopImages(IEnumerable<string> searchIds)
            => await SynchronizeEntityImages<IWorkshopRepository, Workshop, Guid>(workshopRepository, searchIds)
                .ConfigureAwait(false);

        private async Task<List<string>> SynchronizeProviderImages(IEnumerable<string> searchIds)
            => await SynchronizeEntityImages<ISensitiveEntityRepository<Teacher>, Teacher, Guid>(teacherRepository, searchIds)
                .ConfigureAwait(false);

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

        // private HashSet<string> SynchronizeEntityImages(IEnumerable<string> searchIds)
        // {
        //     return workshopRepository
        //         .GetByFilterNoTracking(x => x.Images.Select(r => r.ExternalStorageId).Intersect(searchIds))
        //         .Select(x => x.CoverImageId)
        //         .ToHashSet();
        // }
    }
}