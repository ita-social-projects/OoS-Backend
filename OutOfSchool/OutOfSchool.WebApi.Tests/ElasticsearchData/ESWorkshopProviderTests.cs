using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.ElasticsearchData;

[TestFixture]
public class ESWorkshopProviderTests
{
    private ESWorkshopProvider provider;
    private Mock<ElasticsearchClient> elasticClientMock;

    [SetUp]
    public void Setup()
    {
        elasticClientMock = new Mock<ElasticsearchClient>();
        provider = new ESWorkshopProvider(elasticClientMock.Object);
    }

    #region IndexEntityAsync

    [TestCase(Result.Created, TestName = "IndexEntityAsync_WhenEntityDoesNotExistInIndex_ShouldReturnCreatedResult")]
    [TestCase(Result.Updated, TestName = "IndexEntityAsync_WhenEntityExistsInIndex_ShouldReturnUpdatedResult")]
    public async Task IndexEntityAsync_ShouldReturnCorrectResult(Result operationResult)
    {
        // Arrange
        var entity = WorkshopESGenerator.Generate();
        var response = new IndexResponse()
        {
            Result = operationResult,
        };
        elasticClientMock.Setup(x => x.IndexAsync(entity, CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.IndexEntityAsync(entity);

        // Assert
        elasticClientMock.Verify(x => x.IndexAsync(entity, CancellationToken.None), Times.Once);
        Assert.IsNotNull(result);
        Assert.AreEqual(operationResult, result);
    }

    #endregion

    #region UpdateEntityAsync

    [TestCase(Result.Updated, TestName = "UpdateEntityAsync_WhenEntityExistsInIndex_ShouldReturnUpdatedResult")]
    [TestCase(Result.Created, TestName = "UpdateEntityAsync_WhenEntityDoesNotExistInIndex_ShouldReturnCreatedResult")]
    [TestCase(Result.NoOp, TestName = "UpdateEntityAsync_WhenEntityIsUpToDate_ShouldReturnNoOpResult")]
    public async Task UpdateEntityAsync_ShouldReturnCorrectResult(Result operationResult)
    {
        // Arrange
        var entity = WorkshopESGenerator.Generate();
        var response = new UpdateResponse<WorkshopES>()
        {
            Result = operationResult,
        };
        elasticClientMock.Setup(
            x => x.UpdateAsync(
                entity,
                entity,
                It.IsAny<Action<UpdateRequestDescriptor<WorkshopES, WorkshopES>>>(),
                CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.UpdateEntityAsync(entity);

        // Assert
        elasticClientMock.Verify(
            x => x.UpdateAsync(
                entity,
                entity,
                It.IsAny<Action<UpdateRequestDescriptor<WorkshopES, WorkshopES>>>(),
                CancellationToken.None),
            Times.Once);
        Assert.IsNotNull(result);
        Assert.AreEqual(operationResult, result);
    }

    #endregion

    #region DeleteEntityAsync

    [TestCase(Result.Deleted, TestName = "DeleteEntityAsync_WhenEntityExistsInIndex_ShouldReturnDeletedResult")]
    [TestCase(Result.NotFound, TestName = "DeleteEntityAsync_WhenEntityDoesNotExistInIndex_ShouldReturnNotFoundResult")]
    public async Task DeleteEntityAsync_ShouldReturnCorrectResult(Result operationResult)
    {
        var entity = WorkshopESGenerator.Generate();
        var response = new DeleteResponse()
        {
            Result = operationResult,
        };
        elasticClientMock.Setup(x => x.DeleteAsync<WorkshopES>(
            It.IsAny<DeleteRequestDescriptor<WorkshopES>>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.DeleteEntityAsync(entity);

        // Assert
        elasticClientMock.Verify(
            x => x.DeleteAsync<WorkshopES>(
            It.IsAny<DeleteRequestDescriptor<WorkshopES>>(), CancellationToken.None),
            Times.Once);
        Assert.IsNotNull(result);
        Assert.AreEqual(operationResult, result);
    }

    #endregion

    #region DeleteRangeOfEntitiesByIdsAsync

    [Test]
    public async Task DeleteRangeOfEntitiesByIdsAsync_WhenBulkIsSuccessful_ShouldReturnDeleted()
    {
        // Arrange
        var response = TestableResponseFactory
            .CreateSuccessfulResponse<BulkResponse>(new(), StatusCodes.Status200OK);
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        elasticClientMock.Setup(x => x.BulkAsync(It.IsAny<BulkRequest>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.DeleteRangeOfEntitiesByIdsAsync(ids);

        // Assert
        elasticClientMock.Verify(
            x => x.BulkAsync(It.IsAny<BulkRequest>(), CancellationToken.None), Times.Once);
        Assert.IsNotNull(result);
        Assert.AreEqual(Result.Deleted, result);
    }

    [Test]
    public async Task DeleteRangeOfEntitiesByIdsAsync_WhenBulkFails_ShouldReturnDeleted()
    {
        // Arrange
        var response = TestableResponseFactory
            .CreateSuccessfulResponse<BulkResponse>(
                new() { Errors = true },
                StatusCodes.Status200OK);
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        elasticClientMock.Setup(x => x.BulkAsync(It.IsAny<BulkRequest>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.DeleteRangeOfEntitiesByIdsAsync(ids);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(Result.Deleted, result);
        elasticClientMock.Verify(
            x => x.BulkAsync(It.IsAny<BulkRequest>(), CancellationToken.None), Times.Once);
    }

    #endregion

    #region ReIndexAll

    [Test]
    public async Task ReIndexAll_WhenReIndexSuccessful_ShouldReturnUpdatedResult()
    {
        var source = WorkshopESGenerator.Generate(5);
        elasticClientMock
            .Setup(x => x.BulkAll(
                source,
                It.IsAny<Action<BulkAllRequestDescriptor<WorkshopES>>>(),
                CancellationToken.None))
            .Returns(new BulkAllObservable<WorkshopES>(
                elasticClientMock.Object,
                new Mock<IBulkAllRequest<WorkshopES>>().Object,
                CancellationToken.None));

        // Act
        var result = await provider.ReIndexAll(source);

        // Assert
        elasticClientMock.Verify(
            x => x.DeleteByQueryAsync<WorkshopES>(
                It.IsAny<DeleteByQueryRequestDescriptor<WorkshopES>>(), CancellationToken.None),
            Times.Once);
        elasticClientMock.Verify(
            x => x.BulkAll(
                source,
                It.IsAny<Action<BulkAllRequestDescriptor<WorkshopES>>>(),
                CancellationToken.None),
            Times.Once);
        Assert.AreEqual(Result.Updated, result);
    }

    [Test]
    public void ReIndexAll_WhenReIndexFails_ShouldThrowException()
    {
        var source = WorkshopESGenerator.Generate(3);
        var exceptionMessage = "Test exception";
        elasticClientMock
            .Setup(x => x.BulkAll(
                source,
                It.IsAny<Action<BulkAllRequestDescriptor<WorkshopES>>>(),
                CancellationToken.None))
            .Throws(new Exception(exceptionMessage));

        // Act & Assert
        Exception ex = Assert.ThrowsAsync<Exception>(
            async () => await provider.ReIndexAll(source));
        Assert.AreEqual(exceptionMessage, ex.Message);
        elasticClientMock.Verify(
            x => x.DeleteByQueryAsync<WorkshopES>(
                It.IsAny<DeleteByQueryRequestDescriptor<WorkshopES>>(), CancellationToken.None),
            Times.Once);
        elasticClientMock.Verify(
            x => x.BulkAll(
                source,
                It.IsAny<Action<BulkAllRequestDescriptor<WorkshopES>>>(),
                CancellationToken.None),
            Times.Once);
    }

    #endregion

    #region Search

    #endregion

}
