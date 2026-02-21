using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ElasticsearchWorkshopControllerTests
{
    private ElasticsearchWorkshopController controller;
    private Mock<IElasticsearchService<WorkshopES, WorkshopFilterES>> elasticsearchServiceMoq;

    [SetUp]
    public void SetUp()
    {
        elasticsearchServiceMoq = new Mock<IElasticsearchService<WorkshopES, WorkshopFilterES>>();

        controller = new ElasticsearchWorkshopController(elasticsearchServiceMoq.Object);
    }

    #region Reindex

    [TestCase(true, StatusCodes.Status200OK)]
    [TestCase(false, StatusCodes.Status500InternalServerError)]
    public async Task Reindex_WhenCalled_ShouldReturnCorrectStatusCode(bool reindexSuccess, int expectedStatusCode)
    {
        // Arrange
        elasticsearchServiceMoq.Setup(x => x.ReIndex()).ReturnsAsync(reindexSuccess);

        // Act
        var result = await controller.ReIndex();

        // Assert
        elasticsearchServiceMoq.Verify(x => x.ReIndex(), Times.Once());
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<StatusCodeResult>(result);
        Assert.AreEqual(expectedStatusCode, result.StatusCode);
    }
    #endregion

    #region Search

    [Test]
    public async Task Search_WhenFilterIsValidAndDataFound_ShouldReturnTotalAmountAndEntities()
    {
        // Arrange
        var filter = new WorkshopFilterES();
        var expectedResult = new SearchResultES<WorkshopES>()
        {
            TotalAmount = 3,
            Entities =
            [
                new WorkshopES(),
                new WorkshopES(),
                new WorkshopES(),
            ],
        };
        elasticsearchServiceMoq.Setup(x => x.Search(filter)).ReturnsAsync(expectedResult);

        // Act
        var result = await controller.Search(filter);

        // Assert
        elasticsearchServiceMoq.Verify(x => x.Search(filter), Times.Once());
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var actualResult = okResult.Value as SearchResultES<WorkshopES>;
        Assert.AreEqual(expectedResult.TotalAmount, actualResult.TotalAmount);
        Assert.AreEqual(expectedResult.Entities.Count, actualResult.Entities.Count);
    }

    [Test]
    public async Task Search_WhenNoDataFound_ShouldReturnEmptyResult()
    {
        // Arrange
        var filter = new WorkshopFilterES();
        elasticsearchServiceMoq.Setup(x => x.Search(filter))
            .ReturnsAsync(new SearchResultES<WorkshopES>() { Entities = [] });

        // Act
        var result = await controller.Search(filter);

        // Assert
        elasticsearchServiceMoq.Verify(x => x.Search(filter), Times.Once());
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var actualResult = okResult.Value as SearchResultES<WorkshopES>;
        Assert.AreEqual(0, actualResult.TotalAmount);
        Assert.AreEqual(0, actualResult.Entities.Count);
    }

    [Test]
    public async Task Search_WhenFilterIsNull_ShouldReturnEmptyResult()
    {
        // Arrange
        WorkshopFilterES filter = null;
        elasticsearchServiceMoq.Setup(x => x.Search(filter))
            .ReturnsAsync(new SearchResultES<WorkshopES>() { Entities = [] });

        // Act
        var result = await controller.Search(filter);

        // Assert
        elasticsearchServiceMoq.Verify(x => x.Search(filter), Times.Once());
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var actualResult = okResult.Value as SearchResultES<WorkshopES>;
        Assert.AreEqual(0, actualResult.TotalAmount);
        Assert.AreEqual(0, actualResult.Entities.Count);
    }
    #endregion
}