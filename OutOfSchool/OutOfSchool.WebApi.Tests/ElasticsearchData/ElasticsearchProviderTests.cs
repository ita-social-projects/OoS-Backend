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

    #region Search

    [Test]
    public async Task Search_WithValidFilter_ShouldReturnSearchResult()
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
        Assert.AreEqual(expectedCount, result.Entities.Count);
        Assert.AreEqual(expectedCount, result.TotalAmount);
    }

    #endregion
}
