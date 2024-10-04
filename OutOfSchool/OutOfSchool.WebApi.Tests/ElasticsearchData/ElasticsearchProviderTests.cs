using System;
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
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.ElasticsearchData;

[TestFixture]
public class ElasticsearchProviderTests
{
    private ElasticsearchProvider<WorkshopES, WorkshopFilterES> provider;
    private Mock<ElasticsearchClient> elasticClientMock;

    [SetUp]
    public void Setup()
    {
        elasticClientMock = new Mock<ElasticsearchClient>();

        provider = new ElasticsearchProvider<WorkshopES, WorkshopFilterES>(
            elasticClientMock.Object);
    }

    #region IndexEntityAsync

    [Test]
    public async Task IndexEntityAsync_WithEntityNotExistedInIndex_ShouldReturnCreated()
    {
        // Arrange
        var entity = WorkshopESGenerator.Generate();
        var response = new IndexResponse()
        {
            Result = Result.Created,
        };
        elasticClientMock.Setup(x => x.IndexAsync(entity, CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.IndexEntityAsync(entity);

        // Assert
        elasticClientMock.Verify(x => x.IndexAsync(entity, CancellationToken.None), Times.Once);
        Assert.IsNotNull(result);
        Assert.AreEqual(Result.Created, result);
    }

    [Test]
    public async Task IndexEntityAsync_WithEntityExistedInIndex_ShouldReturnUpdated()
    {
        // Arrange
        var entity = WorkshopESGenerator.Generate();
        var response = new IndexResponse()
        {
            Result = Result.Updated,
        };
        elasticClientMock.Setup(x => x.IndexAsync(entity, CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.IndexEntityAsync(entity);

        // Assert
        elasticClientMock.Verify(x => x.IndexAsync(entity, CancellationToken.None), Times.Once);
        Assert.IsNotNull(result);
        Assert.AreEqual(Result.Updated, result);
    }

    #endregion

    #region Search

    [Test]
    public async Task Search_ShouldReturnSearchResult()
    {
        // Arrange
        var expectedCount = 2;
        var response = new SearchResponse<WorkshopES>
        {
             HitsMetadata = new HitsMetadata<WorkshopES>
             {
                 Hits = new List<Hit<WorkshopES>>(
                     Enumerable.Repeat(new Hit<WorkshopES>(), expectedCount)),
                 Total = new TotalHits { Value = expectedCount },
             },
        };
        var searchResponse = TestableResponseFactory
            .CreateSuccessfulResponse<SearchResponse<WorkshopES>>(
            response, StatusCodes.Status200OK);
        elasticClientMock.Setup(x => x.SearchAsync<WorkshopES>(
            It.IsAny<Action<SearchRequestDescriptor<WorkshopES>>>(),
            CancellationToken.None))
            .ReturnsAsync(searchResponse);

        // Act
        var result = await provider.Search();

        // Assert
        elasticClientMock.Verify(
            x => x.SearchAsync<WorkshopES>(
            It.IsAny<Action<SearchRequestDescriptor<WorkshopES>>>(),
            CancellationToken.None),
            Times.Once);
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedCount, result.Entities.Count);
        Assert.AreEqual(expectedCount, result.TotalAmount);
    }

    #endregion
}
