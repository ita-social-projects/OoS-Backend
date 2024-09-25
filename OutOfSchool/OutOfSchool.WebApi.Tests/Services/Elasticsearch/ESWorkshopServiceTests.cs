using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Elasticsearch;

[TestFixture]
public class ESWorkshopServiceTests
{
    private ESWorkshopService service;
    private Mock<IWorkshopService> workshopServiceMock;
    private Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>> esProviderMock;
    private Mock<IElasticsearchHealthService> elasticHealthServiceMock;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IMapper> mapperMock;

    [SetUp]
    public void Setup()
    {
        workshopServiceMock = new Mock<IWorkshopService>();
        esProviderMock = new Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>>();
        elasticHealthServiceMock = new Mock<IElasticsearchHealthService>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        mapperMock = new Mock<IMapper>();
        service = new ESWorkshopService(
            workshopServiceMock.Object,
            esProviderMock.Object,
            elasticHealthServiceMock.Object,
            new Mock<ILogger<ESWorkshopService>>().Object,
            averageRatingServiceMock.Object,
            mapperMock.Object);
    }

    #region Index

    [TestCase(Result.Updated)]
    [TestCase(Result.Created)]
    public async Task Index_WhenEntityIndexed_ReturnsTrue(Result operationResult)
    {
        // Arrange
        var entity = WorkshopESGenerator.Generate();
        esProviderMock.Setup(x => x.IndexEntityAsync(entity))
            .ReturnsAsync(operationResult);

        // Act
        var result = await service.Index(entity);

        // Assert
        esProviderMock.Verify(x => x.IndexEntityAsync(entity), Times.Once());
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
    }

    [TestCase(Result.NotFound)]
    [TestCase(Result.NoOp)]
    [TestCase(Result.Deleted)]
    public async Task Index_WhenEntityNotIndexed_ReturnsFalse(Result operationResult)
    {
        // Arrange
        var entity = WorkshopESGenerator.Generate();
        esProviderMock.Setup(x => x.IndexEntityAsync(entity))
            .ReturnsAsync(operationResult);

        // Act
        var result = await service.Index(entity);

        // Assert
        esProviderMock.Verify(x => x.IndexEntityAsync(entity), Times.Once());
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
    }

    [Test]
    public void Index_WhenEntityIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.Index((WorkshopES)null));
    }

    #endregion

    #region Update

    [TestCase(Result.Updated, true)]
    [TestCase(Result.Created, true)]
    [TestCase(Result.Updated, false)]
    [TestCase(Result.Created, false)]
    public async Task Update_WhenEntityUpdated_ReturnsTrue(
        Result operationResult, bool hasRating)
    {
        // Arrange
        var entity = WorkshopESGenerator.Generate();
        AverageRatingDto rating = hasRating ?
            RatingsGenerator.GetAverageRating(entity.Id) : null;
        averageRatingServiceMock.Setup(x => x.GetByEntityIdAsync(entity.Id))
            .ReturnsAsync(rating);
        esProviderMock.Setup(x => x.UpdateEntityAsync(entity))
            .ReturnsAsync(operationResult);

        // Act
        var result = await service.Update(entity);

        // Assert
        averageRatingServiceMock.Verify(x => x.GetByEntityIdAsync(entity.Id), Times.Once());
        esProviderMock.Verify(x => x.UpdateEntityAsync(entity), Times.Once());
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
    }

    [TestCase(Result.NotFound, true)]
    [TestCase(Result.NoOp, true)]
    [TestCase(Result.Deleted, true)]
    [TestCase(Result.NotFound, false)]
    [TestCase(Result.NoOp, false)]
    [TestCase(Result.Deleted, false)]
    public async Task Update_WhenEntityNotUpdated_ReturnsFalse(
    Result operationResult, bool hasRating)
    {
        // Arrange
        var entity = WorkshopESGenerator.Generate();
        AverageRatingDto rating = hasRating ?
            RatingsGenerator.GetAverageRating(entity.Id) : null;
        averageRatingServiceMock.Setup(x => x.GetByEntityIdAsync(entity.Id))
            .ReturnsAsync(rating);
        esProviderMock.Setup(x => x.UpdateEntityAsync(entity))
            .ReturnsAsync(operationResult);

        // Act
        var result = await service.Update(entity);

        // Assert
        averageRatingServiceMock.Verify(x => x.GetByEntityIdAsync(entity.Id), Times.Once());
        esProviderMock.Verify(x => x.UpdateEntityAsync(entity), Times.Once());
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
    }

    [Test]
    public void Update_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.Update((WorkshopES)null));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_WhenEntityDeleted_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        esProviderMock.Setup(x => x.DeleteEntityAsync(It.Is<WorkshopES>(w => w.Id == id)))
            .ReturnsAsync(Result.Deleted);

        // Act
        var result = await service.Delete(id);

        // Assert
        esProviderMock.Verify(
            x => x.DeleteEntityAsync(It.Is<WorkshopES>(w => w.Id == id)),
            Times.Once());
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
    }

    [TestCase(Result.Updated)]
    [TestCase(Result.NotFound)]
    [TestCase(Result.NoOp)]
    [TestCase(Result.Created)]
    public async Task Delete_WhenEntityNotDeleted_ReturnsFalse(Result operationResult)
    {
        // Arrange
        var id = Guid.NewGuid();
        esProviderMock.Setup(x => x.DeleteEntityAsync(It.Is<WorkshopES>(w => w.Id == id)))
            .ReturnsAsync(operationResult);

        // Act
        var result = await service.Delete(id);

        // Assert
        esProviderMock.Verify(
            x => x.DeleteEntityAsync(It.Is<WorkshopES>(w => w.Id == id)),
            Times.Once());
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
    }

    #endregion

    #region ReIndex

    [TestCase(true)]
    [TestCase(false)]
    public async Task ReIndex_SuccessfulReindexing_ReturnsTrue(bool hasRating)
    {
        // Arrange
        var workshops = WorkshopDtoGenerator.Generate(5);
        SearchResult<WorkshopDto> searchResult = new()
        {
            Entities = workshops,
            TotalAmount = workshops.Count,
        };
        workshopServiceMock.Setup(x => x.GetAll(It.IsAny<OffsetFilter>()))
            .ReturnsAsync(searchResult)
            .Callback<OffsetFilter>(filter =>
            {
                if (filter.From > 0)
                {
                    searchResult.Entities = [];
                }
            });
        averageRatingServiceMock.Setup(x => x.GetByEntityIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(hasRating ?
                RatingsGenerator.GetAverageRating(Guid.NewGuid()) : null);
        mapperMock.Setup(x => x.Map<WorkshopES>(It.IsAny<WorkshopDto>()))
            .Returns(new WorkshopES());
        esProviderMock.Setup(x => x.ReIndexAll(It.IsAny<List<WorkshopES>>()))
            .ReturnsAsync(Result.Updated);

        // Act
        var result = await service.ReIndex();

        // Assert
        workshopServiceMock.Verify(
            x => x.GetAll(It.IsAny<OffsetFilter>()), Times.Exactly(2));
        averageRatingServiceMock.Verify(
            x => x.GetByEntityIdAsync(It.IsAny<Guid>()), Times.Exactly(workshops.Count));
        mapperMock.Verify(
            x => x.Map<WorkshopES>(It.IsAny<WorkshopDto>()), Times.Exactly(workshops.Count));
        esProviderMock.Verify(x => x.ReIndexAll(It.IsAny<List<WorkshopES>>()), Times.Once);
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ReIndex_ReindexingFails_ReturnsFalse()
    {
        // Arrange
        var workshops = WorkshopDtoGenerator.Generate(3);
        SearchResult<WorkshopDto> searchResult = new()
        {
            Entities = workshops,
            TotalAmount = workshops.Count,
        };
        workshopServiceMock.Setup(x => x.GetAll(It.IsAny<OffsetFilter>()))
            .ReturnsAsync(searchResult)
            .Callback<OffsetFilter>(filter =>
            {
                if (filter.From > 0)
                {
                    searchResult.Entities = [];
                }
            });
        averageRatingServiceMock.Setup(x => x.GetByEntityIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(RatingsGenerator.GetAverageRating(Guid.NewGuid()));
        mapperMock.Setup(x => x.Map<WorkshopES>(It.IsAny<WorkshopDto>()))
            .Returns(new WorkshopES());
        esProviderMock.Setup(x => x.ReIndexAll(It.IsAny<List<WorkshopES>>()))
            .ReturnsAsync(Result.NoOp);

        // Act
        var result = await service.ReIndex();

        // Assert
        workshopServiceMock.Verify(
            x => x.GetAll(It.IsAny<OffsetFilter>()), Times.Exactly(2));
        averageRatingServiceMock.Verify(
            x => x.GetByEntityIdAsync(It.IsAny<Guid>()), Times.Exactly(workshops.Count));
        mapperMock.Verify(
            x => x.Map<WorkshopES>(It.IsAny<WorkshopDto>()), Times.Exactly(workshops.Count));
        esProviderMock.Verify(x => x.ReIndexAll(It.IsAny<List<WorkshopES>>()), Times.Once);
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ReIndex_ExceptionThrown_ReturnFalse()
    {
        // Arrange
        workshopServiceMock.Setup(x => x.GetAll(It.IsAny<OffsetFilter>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await service.ReIndex();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
    }

    #endregion

    #region Search

    [Test]
    public async Task Search_SuccessfulSearch_ReturnsResult()
    {
        // Arrange
        var filter = new WorkshopFilterES();
        var entities = WorkshopESGenerator.Generate(3);
        var expectedResult = new SearchResultES<WorkshopES>()
        {
            Entities = entities,
            TotalAmount = entities.Count,
        };
        esProviderMock.Setup(x => x.Search(filter)).ReturnsAsync(expectedResult);

        // Act
        var result = await service.Search(filter);

        // Assert
        esProviderMock.Verify(x => x.Search(filter), Times.Once);
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<SearchResultES<WorkshopES>>(result);
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task Search_ThrownException_ReturnsEmptyResult()
    {
        // Arrange
        var filter = new WorkshopFilterES();
        esProviderMock.Setup(x => x.Search(filter))
            .ThrowsAsync(new Exception("Search failed"));

        // Act
        var result = await service.Search(filter);

        // Assert
        esProviderMock.Verify(x => x.Search(filter), Times.Once);
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<SearchResultES<WorkshopES>>(result);
        Assert.AreEqual(0, result.TotalAmount);
        Assert.IsNull(result.Entities);
    }

    #endregion
}
