using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Repository.Files;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Common.Synchronization;
using OutOfSchool.WebApi.Services.Gcp;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace OutOfSchool.WebApi.Tests.Services.Gcp
{
    [TestFixture]
    public class GcpImagesStorageSynchronizationServiceTests
    {
        private static readonly DateTime DefaultCreatedMinTime = DateTime.UtcNow.AddYears(-1);
        private static readonly DateTime DefaultCreatedMaxTime = DateTime.UtcNow.AddMinutes(GapConstants.GcpImagesSynchronizationDateTimeAddMinutesGap);

        private IGcpStorageSynchronizationService gcpStorageSynchronizationService;
        private Mock<ILogger<GcpImagesStorageSynchronizationService>> loggerMock;
        private Mock<IImageFilesStorage> imageFilesStorageMock;
        private Mock<IGcpImagesSyncDataRepository> gcpImagesSyncDataRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            loggerMock = new Mock<ILogger<GcpImagesStorageSynchronizationService>>();
            imageFilesStorageMock = new Mock<IImageFilesStorage>();
            gcpImagesSyncDataRepositoryMock = new Mock<IGcpImagesSyncDataRepository>();
            gcpStorageSynchronizationService = new GcpImagesStorageSynchronizationService(
                loggerMock.Object,
                imageFilesStorageMock.Object,
                gcpImagesSyncDataRepositoryMock.Object);
        }

        [Test]
        public async Task SynchronizeAsync_WhenFilesExistsWithValidTimeAndEqualInDatabaseAndGcp()
        {
            // Arrange
            var gcpObjects = CreateGcpObjects(5);
            var dbFileIds = gcpObjects.Items.Select(x => x.Name).ToList();
            SetupDefaultImageFilesStorageMock(gcpObjects);
            SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

            // Act
            await gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            gcpObjects.Items.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
        }

        [Test]
        public async Task SynchronizeAsync_WhenSomeFilesDoesNotMatchDefaultCreatedMaxTimeGap()
        {
            // Arrange
            var gcpObjects = CreateGcpObjects(5);
            gcpObjects.Items[0].TimeCreated = DateTime.UtcNow;
            gcpObjects.Items[3].TimeCreated = DateTime.UtcNow;
            var dbFileIds = gcpObjects.Items.Select(x => x.Name).ToList();
            SetupDefaultImageFilesStorageMock(gcpObjects);
            SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

            // Act
            await gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            gcpObjects.Items.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
        }

        [Test]
        public async Task SynchronizeAsync_WhenSomeFilesIsMissingInDatabaseButExistInGcp()
        {
            // Arrange
            var gcpObjects = CreateGcpObjects(5);
            var dbFileIds = gcpObjects.Items.Select(x => x.Name).ToList();
            dbFileIds.RemoveAt(1);
            dbFileIds.RemoveAt(3);
            SetupDefaultImageFilesStorageMock(gcpObjects);
            SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

            // Act
            await gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            gcpObjects.Items.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
        }

        [Test]
        public async Task SynchronizeAsync_WhenSomeFilesIsMissingInGcpButExistInDatabase()
        {
            // Arrange
            var gcpObjects = CreateGcpObjects(5);
            var dbFileIds = gcpObjects.Items.Select(x => x.Name).ToList();
            gcpObjects.Items.RemoveAt(0);
            gcpObjects.Items.RemoveAt(2);
            var idsExpectedResult = dbFileIds.Intersect(gcpObjects.Items.Select(x => x.Name));
            SetupDefaultImageFilesStorageMock(gcpObjects);
            SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

            // Act
            await gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            gcpObjects.Items.Select(x => x.Name).Should().BeEquivalentTo(idsExpectedResult);
        }

        [Test]
        public async Task SynchronizeAsync_WhenGcpItemsIsEmpty()
        {
            // Arrange
            var gcpObjects = new Objects { Items = new List<Object>() };
            var dbFileIds = ImagesGenerator.CreateRandomImageIds(3);
            const int countResult = 0;
            SetupDefaultImageFilesStorageMock(gcpObjects);
            SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

            // Act
            await gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            gcpObjects.Items.Select(x => x.Name).Should().HaveCount(countResult);
        }

        [Test]
        public void SynchronizeAsync_WhenGcpItemsIsNull()
        {
            // Arrange
            var gcpObjects = new Objects { Items = null };
            var dbFileIds = ImagesGenerator.CreateRandomImageIds(3);
            imageFilesStorageMock.Setup(x => x.GetBulkListsOfObjectsAsync(It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
                .Returns(CreateAsyncEnumerableGcpObjects(gcpObjects));
            imageFilesStorageMock.Setup(x => x.DeleteAsync(It.IsAny<string>(), CancellationToken.None))
                .Callback<string, CancellationToken>((fileId, cancellationToken) => DeleteObjectWithName(gcpObjects, fileId));
            SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

            // Act
            Func<Task> act = () => gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            act.Should().NotThrowAsync();
            gcpObjects.Items.Should().BeNull();
        }

        [Test]
        public async Task SynchronizeAsync_WhenDatabaseDoesNotContainsElements()
        {
            // Arrange
            var gcpObjects = CreateGcpObjects(5);
            var dbFileIds = new List<string>();
            SetupDefaultImageFilesStorageMock(gcpObjects);
            SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

            // Act
            await gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            gcpObjects.Items.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
        }

        [Test]
        public async Task SynchronizeAsync_WhenSomeFilesIsMissingInDatabaseButExistInGcp_WithLotsOfObjects()
        {
            // Arrange
            var gcpObjects = CreateGcpObjects(1000);
            var dbFileIds = gcpObjects.Items.Where((x, i) => i % 2 != 0).Select(x => x.Name).ToList();
            SetupDefaultImageFilesStorageMock(gcpObjects);
            SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

            // Act
            await gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            gcpObjects.Items.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
        }

        [Test]
        public async Task SynchronizeAsync_WhenGcpSyncDataRepositoryReturnsDifferentIntersects()
        {
            // Arrange
            const byte countOfElements = 9;
            const byte takeOffset = countOfElements / 3;
            var gcpObjects = CreateGcpObjects(countOfElements);
            var dbFileIds = gcpObjects.Items.Select(x => x.Name).ToList();
            var dbProviderFileIds = dbFileIds.Take(takeOffset).ToList();
            var dbProviderCoverFileIds = dbFileIds.Skip(takeOffset).Take(takeOffset).ToList();
            var idsExpectedResult = dbProviderFileIds.Union(dbProviderCoverFileIds);

            SetupDefaultImageFilesStorageMock(gcpObjects);
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectProviderImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, dbProviderFileIds));
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectWorkshopImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, new List<string>()));
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectProviderCoverImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, dbProviderCoverFileIds));
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectTeacherCoverImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, new List<string>()));
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectWorkshopCoverImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, new List<string>()));

            // Act
            await gcpStorageSynchronizationService.SynchronizeAsync();

            // Assert
            Console.WriteLine(dbFileIds.Count);
            gcpObjects.Items.Select(x => x.Name).Should().BeEquivalentTo(idsExpectedResult);
        }

        // Maybe create objects' builder in images generator in case if it needs a lot of properties to be initialized
        private static Objects CreateGcpObjects(int count)
        {
            var objects = new Objects { Items = ImagesGenerator.CreateGcpEmptyObjects(count) };
            var randomIds = ImagesGenerator.CreateRandomImageIds(count);
            var createDateTimes = ImagesGenerator.CreateRandomImageDateTimes(count, DefaultCreatedMinTime, DefaultCreatedMaxTime);

            for (var i = 0; i < count; i++)
            {
                objects.Items[i].Name = randomIds[i];
                objects.Items[i].TimeCreated = createDateTimes[i];
            }

            return objects;
        }

        private static async IAsyncEnumerable<Objects> CreateAsyncEnumerableGcpObjects(Objects objects)
        {
            await Task.CompletedTask;
            yield return objects;
        }

        private static void DeleteObjectWithName(Objects objects, string name)
        {
            var objectWithName = objects.Items.First(x => x.Name.Equals(name));
            objects.Items.Remove(objectWithName);
        }

        private static Task<List<string>> GetIntersectWithDbIds(IEnumerable<string> searchIds, IEnumerable<string> dbIds)
            => Task.FromResult(searchIds.Intersect(dbIds).ToList());

        private void SetupDefaultImageFilesStorageMock(Objects objects)
        {
            var objectNames = objects.Items.Select(x => x.Name);
            imageFilesStorageMock.Setup(x => x.GetBulkListsOfObjectsAsync(It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
                .Returns(CreateAsyncEnumerableGcpObjects(objects));
            imageFilesStorageMock.Setup(x => x.DeleteAsync(It.Is<string>(id => objectNames.Contains(id)), CancellationToken.None))
                .Callback<string, CancellationToken>((fileId, cancellationToken) => DeleteObjectWithName(objects, fileId));
        }

        private void SetupDefaultGcpImagesSyncDataRepositoryMock(List<string> dbIds)
        {
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectProviderImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, dbIds));
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectWorkshopImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, dbIds));
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectProviderCoverImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, dbIds));
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectTeacherCoverImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, dbIds));
            gcpImagesSyncDataRepositoryMock.Setup(x => x.GetIntersectWorkshopCoverImagesIds(It.IsAny<IEnumerable<string>>()))
                .Returns<IEnumerable<string>>(ids => GetIntersectWithDbIds(ids, dbIds));
        }
    }
}