using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Enums;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopControllerTests
{
    private const int Ok = 200;
    private const int NoContent = 204;
    private const int Create = 201;
    private const int BadRequest = 400;
    private const int Forbidden = 403;

    private static List<WorkshopDto> workshops;
    private static List<WorkshopCard> workshopCards;
    private static WorkshopDto workshop;
    private static WorkshopBaseDto workshopCreateDto;
    private static WorkshopBaseDto workshopUpdateDto;
    private static ProviderDto provider;
    private static Mock<IOptions<AppDefaultsConfig>> options;

    private WorkshopController controller;
    private Mock<IWorkshopServicesCombiner> workshopServiceMoq;
    private Mock<IProviderService> providerServiceMoq;
    private Mock<IProviderAdminService> providerAdminService;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<IUserService> userServiceMoq;

    private string userId;
    private Mock<HttpContext> httpContextMoq;
    private List<WorkshopBaseCard> workshopBaseCards;
    private List<ShortEntityDto> workshopShortEntitiesList;
    private List<WorkshopProviderViewCard> workshopProviderViewCardList;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        userId = "someUserId";
        httpContextMoq = new Mock<HttpContext>();
        httpContextMoq.Setup(x => x.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        httpContextMoq.Setup(x => x.User.IsInRole("provider"))
            .Returns(true);

        workshops = WorkshopDtoGenerator.Generate(5);
        workshop = WorkshopDtoGenerator.Generate();
        workshopCreateDto = WorkshopBaseDtoGenerator.Generate();
        workshopUpdateDto = WorkshopBaseDtoGenerator.Generate();
        provider = ProviderDtoGenerator.Generate();
        workshopCards = WorkshopCardGenerator.Generate(5);
        workshopBaseCards = WorkshopBaseCardGenerator.Generate(5);
        workshopShortEntitiesList = ShortEntityDtoGenerator.Generate(10);
        workshopProviderViewCardList = WorkshopProviderViewCardGenerator.Generate(5);

        var config = new AppDefaultsConfig();
        config.City = "Київ";
        options = new Mock<IOptions<AppDefaultsConfig>>();
        options.Setup(x => x.Value).Returns(config);
    }

    [SetUp]
    public void Setup()
    {
        workshopServiceMoq = new Mock<IWorkshopServicesCombiner>();
        providerServiceMoq = new Mock<IProviderService>();
        providerAdminService = new Mock<IProviderAdminService>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        providerAdminService = new Mock<IProviderAdminService>();
        userServiceMoq = new Mock<IUserService>();

        controller = new WorkshopController(
            workshopServiceMoq.Object,
            providerServiceMoq.Object,
            providerAdminService.Object,
            userServiceMoq.Object,
            localizer.Object,
            options.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
        };
    }

    #region GetWorkshopById
    [Test]
    public async Task GetWorkshopById_WhenIdIsValid_ShouldReturnOkResultObject()
    {
        // Arrange
        workshopServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);

        // Act
        var result = await controller.GetById(workshop.Id).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
    }

    [Test]
    public async Task GetWorkshopById_WhenThereIsNoWorkshopWithId_ShouldReturnNoContent()
    {
        // Arrange
        workshopServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync((WorkshopDto)null);

        // Act
        var result = await controller.GetById(workshop.Id).ConfigureAwait(false) as NoContentResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }
    #endregion

    #region GetByProviderId
    [Test]
    public async Task GetByProviderId_WhenThereAreWorkshops_ShouldReturnOkResultObject()
    {
        // Arrange
        var filter = new ExcludeIdFilter() { From = 0, Size = int.MaxValue };
        var searchResult = new SearchResult<WorkshopBaseCard>() { TotalAmount = 5, Entities = workshopBaseCards };
        workshopServiceMoq.Setup(x => x.GetByProviderId<WorkshopBaseCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await controller.GetByProviderId(Guid.NewGuid(), filter).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(workshops.Count, (result.Value as SearchResult<WorkshopBaseCard>).TotalAmount);
    }

    [Test]
    public async Task GetByProviderId_WhenThereIsNoWorkshops_ShouldReturnNoContentResult([Random(uint.MinValue, uint.MaxValue, 1)] long randomNumber)
    {
        // Arrange
        var filter = new ExcludeIdFilter() { From = 0, Size = int.MaxValue };
        var emptySearchResult = new SearchResult<WorkshopBaseCard>() { TotalAmount = 0, Entities = new List<WorkshopBaseCard>() };
        workshopServiceMoq.Setup(x => x.GetByProviderId<WorkshopBaseCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(emptySearchResult);

        // Act
        var result = await controller.GetByProviderId(Guid.NewGuid(), filter).ConfigureAwait(false) as NoContentResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }

    [Test]
    public async Task GetByProviderId_WhenThereIsEmptyGuid_ShouldReturnBadRequest()
    {
        // Act
        var result = await controller.GetByProviderId(Guid.Empty, null).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        Assert.AreEqual("Provider id is empty.", (result as BadRequestObjectResult).Value);
    }

    [Test]
    public async Task GetByProviderId_WhenThereIsExcludedId_ShouldReturnOkResultObject()
    {
        // Arrange
        var expectedWorkshopCount = workshopBaseCards.Count - 1;
        var excludedId = workshopBaseCards.FirstOrDefault().WorkshopId;
        var filter = new ExcludeIdFilter() { From = 0, Size = int.MaxValue, ExcludedId = excludedId };
        var searchResult = new SearchResult<WorkshopBaseCard>() { TotalAmount = 4, Entities = workshopBaseCards.Skip(1).ToList() };
        workshopServiceMoq.Setup(x => x.GetByProviderId<WorkshopBaseCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await controller.GetByProviderId(Guid.NewGuid(), filter).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(expectedWorkshopCount, (result.Value as SearchResult<WorkshopBaseCard>).TotalAmount);
    }

    #endregion

    #region GetWorkshopListByProviderId
    [Test]
    public async Task GetWorkshopListByProviderId_WhenThereAreWorkshops_ShouldReturnOkResultObject()
    {
        // Arrange
        workshopServiceMoq.Setup(x => x.GetWorkshopListByProviderId(It.IsAny<Guid>()))
            .ReturnsAsync(workshopShortEntitiesList);

        // Act
        var result = await controller.GetWorkshopListByProviderId(Guid.NewGuid()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(workshopShortEntitiesList.Count, (result.Value as List<ShortEntityDto>).Count);
    }

    [Test]
    public async Task GetWorkshopListByProviderId_WhenThereIsNoWorkshops_ShouldReturnNoContentResult()
    {
        // Arrange
        var emptyList = new List<ShortEntityDto>();
        workshopServiceMoq.Setup(x => x.GetWorkshopListByProviderId(It.IsAny<Guid>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await controller.GetWorkshopListByProviderId(Guid.NewGuid()).ConfigureAwait(false) as NoContentResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }

    [Test]
    public async Task GetWorkshopListByProviderId_WhenSizeFilterIsProvided_ShouldReturnOkResultObject()
    {
        // Arrange
        var expectedCount = 1;
        workshopServiceMoq.Setup(x => x.GetWorkshopListByProviderId(It.IsAny<Guid>()))
            .ReturnsAsync(workshopShortEntitiesList.Take(expectedCount).ToList());

        // Act
        var result = await controller.GetWorkshopListByProviderId(Guid.NewGuid()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(expectedCount, (result.Value as List<ShortEntityDto>).Count);
    }

    [Test]
    public async Task GetWorkshopListByProviderId_WhenFromFilterIsProvided_ShouldReturnOkResultObject()
    {
        // Arrange
        var skipCount = 1;
        var expectedCount = 2;
        var expectedResult = workshopShortEntitiesList.Skip(skipCount).Take(expectedCount).ToList();
        workshopServiceMoq.Setup(x => x.GetWorkshopListByProviderId(It.IsAny<Guid>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await controller.GetWorkshopListByProviderId(Guid.NewGuid()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(expectedCount, (result.Value as List<ShortEntityDto>).Count);
        Assert.AreSame(expectedResult, result.Value as List<ShortEntityDto>);
    }

    [Test]
    public async Task GetWorkshopListByProviderId_WhenProviderIdIsEmpty_ShouldReturnNoContentResult()
    {
        // Act
        IActionResult result = await controller.GetWorkshopListByProviderId(Guid.Empty).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        Assert.AreEqual("Provider id is empty.", (result as BadRequestObjectResult).Value);
    }
    #endregion

    #region GetWorkshopsByFilter
    [Test]
    public async Task GetWorkshopByFilter_WhenThereAreWorkshops_ShouldReturnOkResultObject()
    {
        // Arrange
        var searchResult = new SearchResult<WorkshopCard>() { TotalAmount = 5, Entities = workshopCards };
        workshopServiceMoq.Setup(x => x.GetByFilter(It.IsAny<WorkshopFilter>())).ReturnsAsync(searchResult);

        // Act
        var result = await controller.GetByFilter(new WorkshopFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.IsInstanceOf<SearchResult<WorkshopCard>>(result.Value);
    }

    [Test]
    public async Task GetWorkshopByFilter_WhenThereIsNoAnyWorkshop_ShouldReturnNoConterntResult()
    {
        // Arrange
        var searchResult = new SearchResult<WorkshopCard>() { TotalAmount = 0, Entities = new List<WorkshopCard>() };
        workshopServiceMoq.Setup(x => x.GetByFilter(It.IsAny<WorkshopFilter>())).ReturnsAsync(searchResult);

        // Act
        var result = await controller.GetByFilter(new WorkshopFilter()).ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }
    #endregion

    #region GetWorkshopProviderViewCardsByProviderId
    [Test]
    public async Task GetWorkshopProviderViewCardsByProviderId_WhenThereAreWorkshops_ShouldReturnOkResultObject()
    {
        // Arrange
        var filter = new ExcludeIdFilter() { From = 0, Size = int.MaxValue };
        var searchResult = new SearchResult<WorkshopProviderViewCard>() { TotalAmount = 5, Entities = workshopProviderViewCardList };
        workshopServiceMoq.Setup(x => x.GetByProviderId<WorkshopProviderViewCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await controller.GetWorkshopProviderViewCardsByProviderId(Guid.NewGuid(), filter).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(workshopProviderViewCardList.Count, (result.Value as SearchResult<WorkshopProviderViewCard>).TotalAmount);
    }

    [Test]
    public async Task GetWorkshopProviderViewCardsByProviderId_WhenThereIsNoWorkshops_ShouldReturnNoContentResult()
    {
        // Arrange
        var filter = new ExcludeIdFilter() { From = 0, Size = int.MaxValue };
        var emptySearchResult = new SearchResult<WorkshopProviderViewCard>() { TotalAmount = 0, Entities = new List<WorkshopProviderViewCard>() };
        workshopServiceMoq.Setup(x => x.GetByProviderId<WorkshopProviderViewCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(emptySearchResult);

        // Act
        var result = await controller.GetWorkshopProviderViewCardsByProviderId(Guid.NewGuid(), filter).ConfigureAwait(false) as NoContentResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }

    [Test]
    public async Task GetWorkshopProviderViewCardsByProviderId_WhenSizeFilterIsProvided_ShouldReturnOkResultObject()
    {
        // Arrange
        var expectedCount = 1;
        var filter = new ExcludeIdFilter() { From = 0, Size = expectedCount };
        var expectedTotalAmount = 5;
        var searchResult = new SearchResult<WorkshopProviderViewCard>() { TotalAmount = expectedTotalAmount, Entities = workshopProviderViewCardList.Take(expectedCount).ToList() };
        workshopServiceMoq.Setup(x => x.GetByProviderId<WorkshopProviderViewCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await controller.GetWorkshopProviderViewCardsByProviderId(Guid.NewGuid(), filter).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(expectedCount, (result.Value as SearchResult<WorkshopProviderViewCard>).Entities.Count);
        Assert.AreEqual(expectedTotalAmount, (result.Value as SearchResult<WorkshopProviderViewCard>).TotalAmount);
    }

    [Test]
    public async Task GetWorkshopProviderViewCardsByProviderId_WhenFromFilterIsProvided_ShouldReturnOkResultObject()
    {
        // Arrange
        var skipCount = 1;
        var expectedCount = 2;
        var expectedTotalAmount = 5;
        var expectedResult = workshopProviderViewCardList.Skip(skipCount).Take(expectedCount).ToList();
        var filter = new ExcludeIdFilter() { From = skipCount, Size = expectedCount };
        var searchResult = new SearchResult<WorkshopProviderViewCard>() { TotalAmount = expectedTotalAmount, Entities = expectedResult };
        workshopServiceMoq.Setup(x => x.GetByProviderId<WorkshopProviderViewCard>(It.IsAny<Guid>(), It.IsAny<ExcludeIdFilter>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await controller.GetWorkshopProviderViewCardsByProviderId(Guid.NewGuid(), filter).ConfigureAwait(false) as OkObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
        Assert.AreEqual(expectedTotalAmount, (result.Value as SearchResult<WorkshopProviderViewCard>).TotalAmount);
        Assert.AreSame(expectedResult, (result.Value as SearchResult<WorkshopProviderViewCard>).Entities);
    }

    [Test]
    public async Task GetWorkshopProviderViewCardsByProviderId_WhenProviderIdIsEmpty_ShouldReturnNoContentResult()
    {
        // Act
        IActionResult result = await controller.GetWorkshopProviderViewCardsByProviderId(Guid.Empty, null).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        Assert.AreEqual("Provider id is empty.", (result as BadRequestObjectResult).Value);
    }
    #endregion

    #region CreateWorkshop
    [Test]
    public async Task CreateWorkshop_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
    {
        // Arrange
        workshopCreateDto.ProviderId = provider.Id;
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(false);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
        workshopServiceMoq.Setup(x => x.Create(workshopCreateDto)).ReturnsAsync(workshopCreateDto);

        // Act
        var result = await controller.Create(workshopCreateDto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.Verify(x => x.Create(workshopCreateDto), Times.Once);
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Create, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenModelIsInvalid_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        workshopServiceMoq.Setup(x => x.Create(workshopCreateDto)).ReturnsAsync(workshopCreateDto);
        controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

        // Act
        var result = await controller.Create(workshopCreateDto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        workshopServiceMoq.Verify(x => x.Create(workshopCreateDto), Times.Never);
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(BadRequest, result.StatusCode);
    }

    [Test]
    public async Task CreateWorkshop_WhenProviderHasNoRights_ShouldReturn403ObjectResult()
    {
        // Arrange
        var notAuthorProvider = new ProviderDto() { Id = It.IsAny<Guid>(), UserId = userId };
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(notAuthorProvider);

        // Act
        var result = await controller.Create(workshopCreateDto) as ObjectResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.Verify(x => x.Create(workshopCreateDto), Times.Never);
        Assert.IsNotNull(result);
        Assert.AreEqual(Forbidden, result.StatusCode);
    }
    #endregion

    #region UpdateWorkshop
    [Test]
    public async Task UpdateWorkshop_WhenModelIsValid_ShouldReturnOkObjectResult()
    {
        // Arrange
        workshopUpdateDto.ProviderId = provider.Id;
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(false);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
        workshopServiceMoq.Setup(x => x.Update(workshopUpdateDto)).ReturnsAsync(workshopUpdateDto);

        // Act
        var result = await controller.Update(workshopUpdateDto).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
    }

    [Test]
    public async Task UpdateWorkshop_WhenModelIsInvalid_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        workshopServiceMoq.Setup(x => x.Update(workshopUpdateDto)).ReturnsAsync(workshopUpdateDto);
        controller.ModelState.AddModelError("CreateWorkshop", "Invalid model state.");

        // Act
        var result = await controller.Update(workshopUpdateDto).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        workshopServiceMoq.Verify(x => x.Update(It.IsAny<WorkshopBaseDto>()), Times.Never);
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(BadRequest, result.StatusCode);
    }

    [Test]
    public async Task UpdateWorkshop_WhenIdProviderHasNoRights_ShouldReturn403ObjectResult()
    {
        // Arrange
        var notAuthorProvider = new ProviderDto() { Id = It.IsAny<Guid>(), UserId = userId };
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(notAuthorProvider);

        // Act
        var result = await controller.Update(workshopUpdateDto).ConfigureAwait(false) as ObjectResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.Verify(x => x.Update(It.IsAny<WorkshopBaseDto>()), Times.Never);
        Assert.IsNotNull(result);
        Assert.AreEqual(Forbidden, result.StatusCode);
    }
    #endregion

    #region UpdateStatus
    [Test]
    public async Task UpdateStatus_WhenModelIsValid_ShouldReturnOkObjectResult()
    {
        // Arrange
        workshop.ProviderId = provider.Id;

        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);

        var updateRequest = WithWorkshopStatusDto(workshop.Id, WorkshopStatus.Open);

        workshopServiceMoq.Setup(x => x.GetById(updateRequest.WorkshopId))
            .ReturnsAsync(workshop);
        workshopServiceMoq.Setup(x => x.UpdateStatus(updateRequest))
            .ReturnsAsync(updateRequest);

        // Act
        var result = await controller.UpdateStatus(updateRequest).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Ok, result.StatusCode);
    }

    [Test]
    public async Task UpdateStatus_WhenIdDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var expected = new NotFoundObjectResult($"There is no Workshop in DB with Id - {nonExistentId}");

        var updateRequest = WithWorkshopStatusDto(nonExistentId, WorkshopStatus.Open);

        workshopServiceMoq.Setup(x => x.GetById(updateRequest.WorkshopId))
            .ReturnsAsync(null as WorkshopDto);

        // Act
        var result = await controller.UpdateStatus(updateRequest).ConfigureAwait(false) as NotFoundObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(expected.Value, result.Value);
    }

    [Test]
    public async Task UpdateStatus_WhenModelIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var updateRequest = WithWorkshopStatusDto(workshop.Id, WorkshopStatus.Closed);

        workshop.ProviderId = provider.Id;
        workshop.ProviderOwnership = OwnershipType.Common;

        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
        workshopServiceMoq.Setup(x => x.GetById(updateRequest.WorkshopId))
            .ReturnsAsync(workshop);
        workshopServiceMoq.Setup(x => x.UpdateStatus(updateRequest)).
            ThrowsAsync(new ArgumentException(It.IsAny<string>()));

        // Act
        var result = await controller.UpdateStatus(updateRequest).ConfigureAwait(false) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(BadRequest, result.StatusCode);
    }

    [Test]
    public async Task UpdateStatus_WhenProviderHasNoRights_ShouldReturn403ObjectResult()
    {
        // Arrange
        var workShopStatusDto = WithWorkshopStatusDto(workshop.Id, WorkshopStatus.Open);
        var notAuthorProvider = new ProviderDto() { Id = It.IsAny<Guid>(), UserId = userId };
        workshopServiceMoq.Setup(x => x.GetById(workShopStatusDto.WorkshopId))
            .ReturnsAsync(workshop);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(notAuthorProvider);

        // Act
        var result = await controller.UpdateStatus(workShopStatusDto) as ObjectResult;

        // Assert
        workshopServiceMoq.Verify(x => x.Create(workshopCreateDto), Times.Never);
        Assert.IsNotNull(result);
        Assert.AreEqual(Forbidden, result.StatusCode);
    }

    #endregion

    #region DeleteWorkshop
    [Test]
    public async Task DeleteWorkshop_WhenIdIsValid_ShouldReturnNoContentResult()
    {
        // Arrange
        workshop.ProviderId = provider.Id;
        workshopServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(provider);
        providerServiceMoq.Setup(x => x.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(false);
        workshopServiceMoq.Setup(x => x.Delete(workshop.Id)).Returns(Task.CompletedTask);

        // Act
        var result = await controller.Delete(workshop.Id) as NoContentResult;

        // Assert
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.VerifyAll();
        workshopServiceMoq.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }

    [Test]
    public async Task DeleteWorkshop_WhenThereIsNoWorkshopWithId_ShouldNoContentResult()
    {
        // Arrange
        workshopServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(() => null);

        // Act
        var result = await controller.Delete(workshop.Id) as NoContentResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        workshopServiceMoq.Verify(x => x.Delete(workshop.Id), Times.Never);
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(NoContent, result.StatusCode);
    }

    [Test]
    public async Task DeleteWorkshop_WhenIdProviderHasNoRights_ShouldReturn403ObjectResult()
    {
        // Arrange
        workshopServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(workshop);
        var notAuthorProvider = new ProviderDto() { Id = It.IsAny<Guid>(), UserId = userId };
        providerServiceMoq.Setup(x => x.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(notAuthorProvider);

        // Act
        var result = await controller.Delete(workshop.Id) as ObjectResult;

        // Assert
        workshopServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();
        workshopServiceMoq.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Never);
        Assert.IsNotNull(result);
        Assert.AreEqual(Forbidden, result.StatusCode);
    }
    #endregion

    private WorkshopStatusDto WithWorkshopStatusDto(Guid workshopDtoId, WorkshopStatus workshopStatus)
    {
        return new WorkshopStatusDto()
        {
            WorkshopId = workshopDtoId,
            Status = workshopStatus,
        };
    }
}