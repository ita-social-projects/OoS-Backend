using Elastic.Clients.Elasticsearch;
using Moq;
using NUnit.Framework;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using System.Threading.Tasks;
using System.Threading;
using Elastic.Transport;
using Microsoft.AspNetCore.Http;
using Elastic.Clients.Elasticsearch.Core.Search;
using System.Collections.Generic;
using System.Linq;
using System;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using Elastic.Clients.Elasticsearch.Aggregations;

namespace OutOfSchool.WebApi.Tests.ElasticsearchData;

[TestFixture]
public class ESCompetitiveEventProviderTests
{
    private ESCompetitiveEventProvider provider;
    private Mock<ElasticsearchClient> elasticClientMock;

    [SetUp]
    public void Setup()
    {
        elasticClientMock = new Mock<ElasticsearchClient>();
        provider = new ESCompetitiveEventProvider(elasticClientMock.Object);
    }

    [Test]
    public async Task Search_WhenFilterIsNull_ShouldReturnSearchResult()
    {
        // Arrange
        var expectedEntities = 12;
        var expectedTotal = 50;

        CompetitiveEventFilterES filter = null;

        var response = CreateSuccessfulSearchResponse(expectedTotal, expectedEntities);

        elasticClientMock.Setup(
            x => x.SearchAsync<CompetitiveEventES>(
                It.IsAny<SearchRequest<CompetitiveEventES>>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.Search(filter);

        // Assert
        elasticClientMock.Verify(
            x => x.SearchAsync<CompetitiveEventES>(
                It.IsAny<SearchRequest<CompetitiveEventES>>(), CancellationToken.None),
            Times.Once);
        Assert.IsInstanceOf<SearchResultES<CompetitiveEventES>>(result);
        Assert.AreEqual(expectedTotal, result.TotalAmount);
        Assert.AreEqual(expectedEntities, result.Entities.Count);
    }

    [Test]
    public async Task Search_WhenFilterContainsIds_ShouldReturnSearchResult()
    {
        // Arrange
        var expectedEntities = 2;
        var expectedTotal = 2;

        CompetitiveEventFilterES filter = new()
        {
            Ids = [Guid.NewGuid(), Guid.NewGuid()],
        };

        var response = CreateSuccessfulSearchResponse(expectedTotal, expectedEntities);

        elasticClientMock.Setup(
            x => x.SearchAsync<CompetitiveEventES>(
                It.IsAny<SearchRequest<CompetitiveEventES>>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.Search(filter);

        // Assert
        elasticClientMock.Verify(
            x => x.SearchAsync<CompetitiveEventES>(
                It.IsAny<SearchRequest<CompetitiveEventES>>(), CancellationToken.None),
            Times.Once);
        Assert.IsInstanceOf<SearchResultES<CompetitiveEventES>>(result);
        Assert.AreEqual(expectedTotal, result.TotalAmount);
        Assert.AreEqual(expectedEntities, result.Entities.Count);
    }

    [Test]
    public async Task Search_WithMultipleFilterParameters_ShouldReturnSearchResult()
    {
        // Arrange
        var expectedEntities = 12;
        var expectedTotal = 20;

        CompetitiveEventFilterES filter = new()
        {
            SearchText = "test",
            MinimumAge = 15,
            MaximumAge = 20,
            OptionsForPeopleWithDisabilities = true,
            PlannedFormatsOfClasses = [FormOfLearning.Offline, FormOfLearning.Mixed],
            States = [CompetitiveEventStates.Published, CompetitiveEventStates.Completed],
            AreThereBenefits = true,
            CompetitiveSelection = true,
            MaxPrice = 1000,
            MaxRegistrationEndTime = DateTime.UtcNow.AddDays(7),
            MaxScheduledStartTime = DateTime.UtcNow.AddDays(7),
        };

        var response = CreateSuccessfulSearchResponse(expectedTotal, expectedEntities);

        elasticClientMock.Setup(
            x => x.SearchAsync<CompetitiveEventES>(
                It.IsAny<SearchRequest<CompetitiveEventES>>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.Search(filter);

        // Assert
        elasticClientMock.Verify(
            x => x.SearchAsync<CompetitiveEventES>(
                It.IsAny<SearchRequest<CompetitiveEventES>>(), CancellationToken.None),
            Times.Once);
        Assert.IsInstanceOf<SearchResultES<CompetitiveEventES>>(result);
        Assert.AreEqual(expectedTotal, result.TotalAmount);
        Assert.AreEqual(expectedEntities, result.Entities.Count);
    }

    [Test]
    public async Task GetPriceRange_WhenFilterIsNull_ShouldReturnPriceRange()
    {
        // Arrange
        var expectedMinPrice = 100;
        var expectedMaxPrice = 200;
        CompetitiveEventFilterES filter = null;

        var aggregations = new Dictionary<string, IAggregate>
        {
            { "min_price", new MinAggregate { Value = expectedMinPrice } },
            { "max_price", new MaxAggregate { Value = expectedMaxPrice } }
        };

        var response = new SearchResponse<CompetitiveEventES>
        {
            Aggregations = new AggregateDictionary(aggregations)
        };

        elasticClientMock.Setup(
            x => x.SearchAsync<CompetitiveEventES>(
                It.IsAny<SearchRequest<CompetitiveEventES>>(), CancellationToken.None))
            .ReturnsAsync(response);

        // Act
        var result = await provider.GetPriceRangeAsync(filter).ConfigureAwait(false);

        // Assert
        elasticClientMock.Verify(
            x => x.SearchAsync<CompetitiveEventES>(
                It.IsAny<SearchRequest<CompetitiveEventES>>(), CancellationToken.None),
            Times.Once);
        Assert.IsInstanceOf<PriceRangeES>(result);
        Assert.AreEqual(expectedMinPrice, result.MinPrice);
        Assert.AreEqual(expectedMaxPrice, result.MaxPrice);
    }

    private static SearchResponse<CompetitiveEventES> CreateSuccessfulSearchResponse(int totalHits, int entities)
    {
        return TestableResponseFactory
            .CreateSuccessfulResponse<SearchResponse<CompetitiveEventES>>(
                new()
                {
                    HitsMetadata = new HitsMetadata<CompetitiveEventES>()
                    {
                        Hits = new List<Hit<CompetitiveEventES>>(
                            Enumerable.Repeat(new Hit<CompetitiveEventES>(), entities)),
                        Total = new TotalHits { Value = totalHits },
                    },
                },
                StatusCodes.Status200OK);
    }
}
