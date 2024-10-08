﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Enums;
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
        var response = TestableResponseFactory
            .CreateSuccessfulResponse<IndexResponse>(
                new() { Result = operationResult }, StatusCodes.Status200OK);
        elasticClientMock.Setup(x => x.IndexAsync(entity, CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.IndexEntityAsync(entity);

        // Assert
        elasticClientMock.Verify(x => x.IndexAsync(entity, CancellationToken.None), Times.Once);
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
        var response = TestableResponseFactory
            .CreateSuccessfulResponse<UpdateResponse<WorkshopES>>(
                new() { Result = operationResult }, StatusCodes.Status200OK);
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
        Assert.AreEqual(operationResult, result);
    }

    #endregion

    #region DeleteEntityAsync

    [TestCase(Result.Deleted, TestName = "DeleteEntityAsync_WhenEntityExistsInIndex_ShouldReturnDeletedResult")]
    [TestCase(Result.NotFound, TestName = "DeleteEntityAsync_WhenEntityDoesNotExistInIndex_ShouldReturnNotFoundResult")]
    public async Task DeleteEntityAsync_ShouldReturnCorrectResult(Result operationResult)
    {
        var entity = WorkshopESGenerator.Generate();
        var response = TestableResponseFactory
            .CreateSuccessfulResponse<DeleteResponse>(
                new() { Result = operationResult }, StatusCodes.Status200OK);
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

    #region IndexAll

    [Test]
    public async Task IndexAll_WhenIndexingSuccessful_ShouldReturnUpdatedResult()
    {
        var source = WorkshopESGenerator.Generate(6);
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
        var result = await provider.IndexAll(source);

        // Assert
        elasticClientMock.Verify(
            x => x.BulkAll(
                source,
                It.IsAny<Action<BulkAllRequestDescriptor<WorkshopES>>>(),
                CancellationToken.None),
            Times.Once);
        Assert.AreEqual(Result.Updated, result);
    }

    [Test]
    public void ReIndexAll_WhenIndexingFails_ShouldThrowException()
    {
        var source = WorkshopESGenerator.Generate(4);
        var exceptionMessage = "Test exception";
        elasticClientMock
            .Setup(x => x.BulkAll(
                source,
                It.IsAny<Action<BulkAllRequestDescriptor<WorkshopES>>>(),
                CancellationToken.None))
            .Throws(new Exception(exceptionMessage));

        // Act & Assert
        Exception ex = Assert.ThrowsAsync<Exception>(
            async () => await provider.IndexAll(source));
        Assert.AreEqual(exceptionMessage, ex.Message);
        elasticClientMock.Verify(
            x => x.BulkAll(
                source,
                It.IsAny<Action<BulkAllRequestDescriptor<WorkshopES>>>(),
                CancellationToken.None),
            Times.Once);
    }

    #endregion

    #region Search

    [Test]
    public async Task Search_WhenFilterIsNull_ShouldReturnSearchResult()
    {
        // Arrange
        var expectedEntities = 7;
        var expectedTotal = 50;

        WorkshopFilterES filter = null;

        var response = TestableResponseFactory
            .CreateSuccessfulResponse<SearchResponse<WorkshopES>>(
                new()
                {
                    HitsMetadata = new HitsMetadata<WorkshopES>()
                    {
                        Hits = new List<Hit<WorkshopES>>(
                            Enumerable.Repeat(new Hit<WorkshopES>(), expectedEntities)),
                        Total = new TotalHits { Value = expectedTotal },
                    },
                },
                StatusCodes.Status200OK);

        elasticClientMock.Setup(
            x => x.SearchAsync<WorkshopES>(
                It.IsAny<SearchRequest<WorkshopES>>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.Search(filter);

        // Assert
        elasticClientMock.Verify(
            x => x.SearchAsync<WorkshopES>(
                It.IsAny<SearchRequest<WorkshopES>>(), CancellationToken.None),
            Times.Once);
        Assert.IsInstanceOf<SearchResultES<WorkshopES>>(result);
        Assert.AreEqual(expectedTotal, result.TotalAmount);
        Assert.AreEqual(expectedEntities, result.Entities.Count);
    }

    [Test]
    public async Task Search_WhenFilterIsValid_ShouldReturnSearchResult()
    {
        // Arrange
        var expectedEntities = 5;
        var expectedTotal = 20;

        WorkshopFilterES filter = new();

        var response = TestableResponseFactory
            .CreateSuccessfulResponse<SearchResponse<WorkshopES>>(
                new()
                {
                    HitsMetadata = new HitsMetadata<WorkshopES>()
                    {
                        Hits = new List<Hit<WorkshopES>>(
                            Enumerable.Repeat(new Hit<WorkshopES>(), expectedEntities)),
                        Total = new TotalHits { Value = expectedTotal },
                    },
                },
                StatusCodes.Status200OK);

        elasticClientMock.Setup(
            x => x.SearchAsync<WorkshopES>(
                It.IsAny<SearchRequest<WorkshopES>>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.Search(filter);

        // Assert
        elasticClientMock.Verify(
            x => x.SearchAsync<WorkshopES>(
                It.IsAny<SearchRequest<WorkshopES>>(), CancellationToken.None),
            Times.Once);
        Assert.IsInstanceOf<SearchResultES<WorkshopES>>(result);
        Assert.AreEqual(expectedTotal, result.TotalAmount);
        Assert.AreEqual(expectedEntities, result.Entities.Count);
    }

    #endregion

    #region PartialUpdateEntityAsync

    [TestCase(Result.Updated, TestName = "PartialUpdateEntityAsync_WhenEntityPartialUpdateSuccessful_ShouldReturnUpdatedResult")]
    [TestCase(Result.NoOp, TestName = "PartialUpdateEntityAsync_WhenEntityIsUpToDate_ShouldReturnNoOpResult")]
    public async Task PartialUpdateEntityAsync_ShouldReturnCorrectResult(Result operationResult)
    {
        // Arrange
        var workshopId = Guid.NewGuid;
        var newWorkshopProviderTitle = new WorkshopProviderTitleES()
        {
            ProviderTitle = "Нова назва",
            ProviderTitleEn = "New title",
        };
        var response = TestableResponseFactory
            .CreateSuccessfulResponse<UpdateResponse<WorkshopES>>(
                new() { Result = operationResult }, StatusCodes.Status200OK);
        elasticClientMock.Setup(
            x => x.UpdateAsync(
                It.IsAny<UpdateRequestDescriptor<WorkshopES, object>>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.PartialUpdateEntityAsync(workshopId, newWorkshopProviderTitle);

        // Assert
        elasticClientMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<UpdateRequestDescriptor<WorkshopES, object>>(), CancellationToken.None),
            Times.Once);
        Assert.AreEqual(operationResult, result);
    }

    #endregion

}