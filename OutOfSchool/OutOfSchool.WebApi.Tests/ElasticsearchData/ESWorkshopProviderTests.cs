using System;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
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

    #endregion

    #region Search

    #endregion
}
