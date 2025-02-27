using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Gcs;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.FileStore;

[TestFixture]
public class ObjectImagesStorageSynchronizationServiceTests
{
    private static readonly DateTimeOffset DefaultCreatedMinTime = DateTimeOffset.UtcNow.AddYears(-1);
    private static readonly DateTimeOffset DefaultCreatedMaxTime = DateTimeOffset.UtcNow.AddMinutes(GapConstants.GcpImagesSynchronizationDateTimeAddMinutesGap);

    private IObjectStorageSynchronizationService objectStorageSynchronizationService;
    private Mock<ILogger<GcsImagesStorageSynchronizationService>> loggerMock;
    private Mock<IObjectImageStorage> imageFilesStorageMock;
    private Mock<IObjectImagesSyncDataRepository> gcpImagesSyncDataRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        loggerMock = new Mock<ILogger<GcsImagesStorageSynchronizationService>>();
        imageFilesStorageMock = new Mock<IObjectImageStorage>();
        gcpImagesSyncDataRepositoryMock = new Mock<IObjectImagesSyncDataRepository>();
        objectStorageSynchronizationService = new GcsImagesStorageSynchronizationService(
            loggerMock.Object,
            imageFilesStorageMock.Object,
            gcpImagesSyncDataRepositoryMock.Object);
    }

    [Test]
    public async Task SynchronizeAsync_WhenFilesExistsWithValidTimeAndEqualInDatabaseAndGcp()
    {
        // Arrange
        var gcpObjects = CreateGcpObjects(5);
        var dbFileIds = gcpObjects.Select(x => x.Name).ToList();
        this.SetupDefaultImageFilesStorageMock(gcpObjects);
        this.SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

        // Act
        await objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        gcpObjects.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
    }

    [Test]
    public async Task SynchronizeAsync_WhenSomeFilesDoesNotMatchDefaultCreatedMaxTimeGap()
    {
        // Arrange
        var gcpObjects = CreateGcpObjects(5);
        gcpObjects[0].CreatedAt = DateTimeOffset.UtcNow;
        gcpObjects[3].CreatedAt = DateTimeOffset.UtcNow;
        var dbFileIds = gcpObjects.Select(x => x.Name).ToList();
        this.SetupDefaultImageFilesStorageMock(gcpObjects);
        this.SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

        // Act
        await objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        gcpObjects.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
    }

    [Test]
    public async Task SynchronizeAsync_WhenSomeFilesIsMissingInDatabaseButExistInGcp()
    {
        // Arrange
        var gcpObjects = CreateGcpObjects(5);
        var dbFileIds = gcpObjects.Select(x => x.Name).ToList();
        dbFileIds.RemoveAt(1);
        dbFileIds.RemoveAt(3);
        var modifiedList = this.SetupDefaultImageFilesStorageMock(gcpObjects);
        this.SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

        // Act
        await objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        modifiedList.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
    }

    [Test]
    public async Task SynchronizeAsync_WhenSomeFilesIsMissingInGcpButExistInDatabase()
    {
        // Arrange
        var gcpObjects = CreateGcpObjects(5);
        var dbFileIds = gcpObjects.Select(x => x.Name).ToList();
        gcpObjects.RemoveAt(0);
        gcpObjects.RemoveAt(2);
        var idsExpectedResult = dbFileIds.Intersect(gcpObjects.Select(x => x.Name));
        this.SetupDefaultImageFilesStorageMock(gcpObjects);
        this.SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

        // Act
        await objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        gcpObjects.Select(x => x.Name).Should().BeEquivalentTo(idsExpectedResult);
    }

    [Test]
    public async Task SynchronizeAsync_WhenGcpItemsIsEmpty()
    {
        // Arrange
        var gcpObjects = new List<StorageObject>();
        var dbFileIds = ImagesGenerator.CreateRandomImageIds(3);
        const int countResult = 0;
        this.SetupDefaultImageFilesStorageMock(gcpObjects);
        this.SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

        // Act
        await objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        gcpObjects.Select(x => x.Name).Should().HaveCount(countResult);
    }

    [Test]
    public void SynchronizeAsync_WhenGcpItemsIsNull()
    {
        // Arrange
        var gcpObjects = new List<StorageObject>();
        var dbFileIds = ImagesGenerator.CreateRandomImageIds(3);
        imageFilesStorageMock.Setup(x => x.ListObjectsAsync(It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
            .Returns(gcpObjects.ToAsyncEnumerable());
        imageFilesStorageMock.Setup(x => x.DeleteAsync(It.IsAny<string>(), CancellationToken.None))
            .Callback<string, CancellationToken>((fileId, cancellationToken) => DeleteObjectWithName(gcpObjects, fileId));
        this.SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

        // Act
        Func<Task> act = () => objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        act.Should().NotThrowAsync();
        gcpObjects.Should().BeEmpty();
    }

    [Test]
    public async Task SynchronizeAsync_WhenDatabaseDoesNotContainsElements()
    {
        // Arrange
        var gcpObjects = CreateGcpObjects(5);
        var dbFileIds = new List<string>();
        var modifiedList = this.SetupDefaultImageFilesStorageMock(gcpObjects);
        this.SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

        // Act
        await objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        modifiedList.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
    }

    [Test]
    public async Task SynchronizeAsync_WhenSomeFilesIsMissingInDatabaseButExistInGcp_WithLotsOfObjects()
    {
        // Arrange
        var gcpObjects = CreateGcpObjects(1000);
        var dbFileIds = gcpObjects.Where((x, i) => i % 2 != 0).Select(x => x.Name).ToList();
        var modifiedList = this.SetupDefaultImageFilesStorageMock(gcpObjects);
        this.SetupDefaultGcpImagesSyncDataRepositoryMock(dbFileIds);

        // Act
        await objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        modifiedList.Select(x => x.Name).Should().BeEquivalentTo(dbFileIds);
    }

    [Test]
    public async Task SynchronizeAsync_WhenGcpSyncDataRepositoryReturnsDifferentIntersects()
    {
        // Arrange
        const byte countOfElements = 9;
        const byte takeOffset = countOfElements / 3;
        var gcpObjects = CreateGcpObjects(countOfElements);
        var dbFileIds = gcpObjects.Select(x => x.Name).ToList();
        var dbProviderFileIds = dbFileIds.Take(takeOffset).ToList();
        var dbProviderCoverFileIds = dbFileIds.Skip(takeOffset).Take(takeOffset).ToList();
        var idsExpectedResult = dbProviderFileIds.Union(dbProviderCoverFileIds);

        var modifiedList = this.SetupDefaultImageFilesStorageMock(gcpObjects);
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
        await objectStorageSynchronizationService.SynchronizeAsync();

        // Assert
        Console.WriteLine(dbFileIds.Count);
        modifiedList.Select(x => x.Name).Should().BeEquivalentTo(idsExpectedResult);
    }

    // Maybe create objects' builder in images generator in case if it needs a lot of properties to be initialized
    private static List<StorageObject> CreateGcpObjects(int count)
    {
        var objects = ImagesGenerator.CreateGcpEmptyObjects(count);
        var randomIds = ImagesGenerator.CreateRandomImageIds(count);
        var createDateTimes = ImagesGenerator.CreateRandomImageDateTimes(count, DefaultCreatedMinTime, DefaultCreatedMaxTime);

        for (var i = 0; i < count; i++)
        {
            objects[i].Name = randomIds[i];
            objects[i].CreatedAt = createDateTimes[i];
        }

        return objects;
    }

    private static void DeleteObjectWithName(List<StorageObject> objects, string name)
    {
        var objectWithName = objects.First(x => x.Name.Equals(name));
        objects.Remove(objectWithName);
    }

    private static Task<List<string>> GetIntersectWithDbIds(IEnumerable<string> searchIds, IEnumerable<string> dbIds)
        => Task.FromResult(searchIds.Intersect(dbIds).ToList());

    private List<StorageObject> SetupDefaultImageFilesStorageMock(List<StorageObject> objects)
    {
        var copy = objects.ToList();
        var objectNames = objects.Select(x => x.Name);
        imageFilesStorageMock.Setup(x => x.ListObjectsAsync(It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
            .Returns(objects.ToAsyncEnumerable());
        imageFilesStorageMock.Setup(x => x.DeleteAsync(It.Is<string>(id => objectNames.Contains(id)), CancellationToken.None))
            .Callback<string, CancellationToken>((fileId, cancellationToken) => DeleteObjectWithName(copy, fileId));
        return copy;
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