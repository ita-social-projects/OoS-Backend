using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class WorkshopServicesCombinerTests
{
    private Mock<IWorkshopService> workshopService;
    private IMapper mapper;
    private Mock<INotificationService> notificationServiceMock;
    private Mock<IEntityRepositorySoftDeleted<long, Favorite>> favoriteRepository;
    private Mock<IApplicationRepository> applicationRepository;
    private Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>> esProvider;
    private Mock<IElasticsearchSynchronizationService> elasticsearchSynchronizationService;
    private IWorkshopServicesCombiner service;

    [SetUp]
    public void SetUp()
    {
        workshopService = new Mock<IWorkshopService>();
        elasticsearchSynchronizationService = new Mock<IElasticsearchSynchronizationService>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();

        favoriteRepository = new Mock<IEntityRepositorySoftDeleted<long, Favorite>>();
        applicationRepository = new Mock<IApplicationRepository>();
        var workshopStrategy = new Mock<IWorkshopStrategy>();
        var currentUserService = new Mock<ICurrentUserService>();
        var ministryAdminService = new Mock<IMinistryAdminService>();
        var regionAdminService = new Mock<IRegionAdminService>();
        var codeficatorService = new Mock<ICodeficatorService>();
        esProvider = new Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>>();

        notificationServiceMock = new Mock<INotificationService>();

        service = new WorkshopServicesCombiner(
            workshopService.Object,
            elasticsearchSynchronizationService.Object,
            notificationServiceMock.Object,
            favoriteRepository.Object,
            applicationRepository.Object,
            workshopStrategy.Object,
            currentUserService.Object,
            ministryAdminService.Object,
            regionAdminService.Object,
            codeficatorService.Object,
            esProvider.Object,
            mapper);
    }

    #region GetById
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task GetById_WithValidId_ShouldReturnDto(bool asNoTracking)
    {
        // Arrange
        var id = Guid.NewGuid();
        var workshopDto = WorkshopDtoGenerator.Generate();
        workshopDto.Id = id;
        workshopService.Setup(x => x.GetById(id, asNoTracking)).ReturnsAsync(workshopDto);

        // Act
        var result = await service.GetById(id, asNoTracking).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(workshopDto, result);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task GetById_WithNotExistId_ShouldReturnNull(bool asNoTracking)
    {
        // Arrange
        var id = Guid.NewGuid();
        var workshopDto = null as WorkshopDto;
        workshopService.Setup(x => x.GetById(id, asNoTracking)).ReturnsAsync(workshopDto);

        // Act
        var result = await service.GetById(id, asNoTracking).ConfigureAwait(false);

        // Assert
        Assert.IsNull(result);
        workshopService.Verify(x => x.GetById(id, asNoTracking), Times.Once);
    }
    #endregion

    #region Create
    [Test]
    public async Task Create_WithValidDto_ShouldReturnSucceededResult()
    {
        // Arrange
        var createdWorkshop = WorkshopGenerator.Generate();
        var workshopCreateRequestDto = mapper.Map<WorkshopCreateRequestDto>(createdWorkshop);
        var workshopDto = mapper.Map<WorkshopDto>(createdWorkshop);
        workshopDto.AvailableSeats = 10;

        workshopService.Setup(x => x.Create(workshopCreateRequestDto))
            .ReturnsAsync(workshopDto).Verifiable(Times.Once);
        elasticsearchSynchronizationService.Setup(
            x => x.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                workshopDto.Id,
                ElasticsearchSyncOperation.Create)).Verifiable(Times.Once);

        // Act
        var result = await service.Create(workshopCreateRequestDto).ConfigureAwait(false);

        // Assert
        workshopService.VerifyAll();
        elasticsearchSynchronizationService.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(workshopDto, result);
    }

    [Test]
    public void Create_WithNotExistedWorkshop_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var workshopCreateRequestDto = (WorkshopCreateRequestDto)null;
        workshopService.Setup(x => x.Create(workshopCreateRequestDto))
            .ThrowsAsync(new ArgumentNullException()).Verifiable(Times.Once);
        elasticsearchSynchronizationService.Setup(
            x => x.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                Guid.NewGuid(),
                ElasticsearchSyncOperation.Create)).Verifiable(Times.Never);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await service.Create(workshopCreateRequestDto));
        workshopService.VerifyAll();
        elasticsearchSynchronizationService.VerifyAll();
    }
    #endregion

    #region Update
    [Test]
    public async Task Update_WithValidDto_ShouldReturnSucceededResult()
    {
        // Arrange
        var currentWorkshopDto = WorkshopDtoGenerator.Generate();
        currentWorkshopDto.TakenSeats = 4;
        var newWorkshopCreateUpdateDto = WorkshopCreateUpdateDtoGenerator.Generate();
        newWorkshopCreateUpdateDto.AvailableSeats = 10;
        workshopService.Setup(x => x.GetById(newWorkshopCreateUpdateDto.Id, true))
            .ReturnsAsync(currentWorkshopDto);

        var updatedWorkshopDto = mapper.Map<WorkshopDto>(newWorkshopCreateUpdateDto);
        workshopService.Setup(x => x.Update(newWorkshopCreateUpdateDto))
            .ReturnsAsync(updatedWorkshopDto);

        // Act
        var result = await service.Update(newWorkshopCreateUpdateDto).ConfigureAwait(false);

        // Assert
        workshopService.VerifyAll();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Succeeded);
        var actual = result.Value;
        Assert.AreEqual(newWorkshopCreateUpdateDto.Title, actual.Title);
        Assert.AreEqual(newWorkshopCreateUpdateDto.ShortTitle, actual.ShortTitle);
        Assert.AreEqual(newWorkshopCreateUpdateDto.Phone, actual.Phone);
        Assert.AreEqual(newWorkshopCreateUpdateDto.Email, actual.Email);
        Assert.AreEqual(newWorkshopCreateUpdateDto.Website, actual.Website);
        Assert.AreEqual(newWorkshopCreateUpdateDto.MinAge, actual.MinAge);
        Assert.AreEqual(newWorkshopCreateUpdateDto.MaxAge, actual.MaxAge);
        Assert.AreEqual(newWorkshopCreateUpdateDto.Price, actual.Price);
        Assert.AreEqual(newWorkshopCreateUpdateDto.ProviderId, actual.ProviderId);
        Assert.AreEqual(newWorkshopCreateUpdateDto.ProviderTitle, actual.ProviderTitle);
        Assert.AreEqual(newWorkshopCreateUpdateDto.AvailableSeats, actual.AvailableSeats);
        Assert.AreEqual(newWorkshopCreateUpdateDto.WithDisabilityOptions, actual.WithDisabilityOptions);
        Assert.AreEqual(newWorkshopCreateUpdateDto.TagIds, actual.Tags.Select(x => x.Id).ToList());
    }

    [Test]
    public async Task Update_WithNotExistWorkshop_ShouldReturnBadRequestResult()
    {
        // Arrange
        var currentWorkshopDto = null as WorkshopDto;
        var newWorkshopCreateUpdateDto = WorkshopCreateUpdateDtoGenerator.Generate();
        workshopService.Setup(x => x.GetById(newWorkshopCreateUpdateDto.Id, true))
            .ReturnsAsync(currentWorkshopDto);

        // Act
        var result = await service.Update(newWorkshopCreateUpdateDto).ConfigureAwait(false);
        var firstError = result.OperationResult.Errors.FirstOrDefault();

        // Assert
        workshopService.VerifyAll();
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Succeeded);
        Assert.IsNotNull(result.OperationResult);
        Assert.IsNotNull(firstError, "Expected an error, but no errors were found.");
        Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), firstError.Code);
        Assert.AreEqual(Constants.WorkshopNotFoundErrorMessage, firstError.Description);
    }

    [Test]
    public async Task Update_WithInvalidAvailableSeats_ShouldReturnBadRequestResult()
    {
        // Arrange
        var currentWorkshopDto = WorkshopDtoGenerator.Generate();
        currentWorkshopDto.TakenSeats = 5;
        var newWorkshopBaseDto = WorkshopCreateUpdateDtoGenerator.Generate();
        newWorkshopBaseDto.AvailableSeats = 3;
        workshopService.Setup(x => x.GetById(newWorkshopBaseDto.Id, true))
            .ReturnsAsync(currentWorkshopDto);

        // Act
        var result = await service.Update(newWorkshopBaseDto).ConfigureAwait(false);
        var firstError = result.OperationResult.Errors.FirstOrDefault();

        // Assert
        workshopService.VerifyAll();
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Succeeded);
        Assert.IsNotNull(result.OperationResult);
        Assert.IsNotNull(firstError, "Expected an error, but no errors were found.");
        Assert.AreEqual(HttpStatusCode.BadRequest.ToString(), firstError.Code);
        Assert.AreEqual(Constants.InvalidAvailableSeatsForWorkshopErrorMessage, firstError.Description);
    }
    #endregion

    #region UpdateStatus
    [Test]
    public async Task UpdateStatus_WhenDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        WorkshopStatusDto workshopStatusDto = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await service.UpdateStatus(workshopStatusDto).ConfigureAwait(false));
    }

    [Test]
    public async Task UpdateStatus_WhenCalled_CreateNotificationWithTitleInAdditionalData()
    {
        // Arrange
        string titleKey = "Title";
        string statusKey = "Status";

        var favorite = new Favorite()
        {
            Id = 1,
            UserId = Guid.NewGuid().ToString(),
            WorkshopId = Guid.NewGuid(),
        };

        var application = ApplicationGenerator.Generate().WithParent(ParentGenerator.Generate());

        var favorites = new List<Favorite>() { favorite };
        var applications = new List<Application>() { application };

        var recipientsIds = new List<string>()
        {
            favorite.UserId,
            application.Parent.UserId,
        };

        var workshop = WorkshopGenerator.Generate();
        workshop.Status = WorkshopStatus.Open;

        var workshopStatusDto = new WorkshopStatusDto()
        {
            WorkshopId = workshop.Id,
            Status = WorkshopStatus.Closed,
        };

        var workshopDto = mapper.Map<WorkshopDto>(workshop);
        var workshopDtoWithTitle = mapper.Map<WorkshopStatusWithTitleDto>(workshopStatusDto);
        workshopDtoWithTitle.Title = workshop.Title;

        workshopService.Setup(x => x.GetById(workshopDto.Id, It.IsAny<bool>())).ReturnsAsync(workshopDto);
        workshopService.Setup(x => x.UpdateStatus(workshopStatusDto)).ReturnsAsync(workshopDtoWithTitle);

        favoriteRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Favorite, object>>, SortDirection>>(),
                It.IsAny<bool>())).Returns(favorites.AsTestAsyncEnumerableQuery());

        applicationRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>())).Returns(applications.AsTestAsyncEnumerableQuery());

        // Act
        await service.UpdateStatus(workshopStatusDto).ConfigureAwait(false);

        // Assert
        notificationServiceMock.Verify(
            x => x.Create(
                NotificationType.Workshop,
                NotificationAction.Update,
                workshop.Id,
                recipientsIds,
                It.Is<Dictionary<string, string>>(c => c.ContainsKey(titleKey) && c.ContainsKey(statusKey)),
                null),
            Times.Once);
    }
    #endregion

    [Test]
    public async Task UpdateProviderStatus_WhenCalled_CallPartialUpdates()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();
        var workshops = ShortEntityDtoGenerator.Generate(3);

        workshopService.Setup(x => x.GetWorkshopListByProviderId(provider.Id)).ReturnsAsync(workshops);

        // Act
        await service.UpdateProviderStatus(provider.Id, provider.Status).ConfigureAwait(false);

        // Assert
        esProvider.Verify(
            x => x.PartialUpdateEntityAsync(
            It.IsAny<Guid>(),
            It.Is<WorkshopProviderStatusES>(c => c.ProviderStatus == provider.Status)),
            Times.Exactly(workshops.Count));
    }

    [Test]
    public async Task Delete_WhenCalled_CreateNotificationWithTitleInAdditionalData()
    {
        // Arrange
        string titleKey = "Title";
        var favorite = new Favorite()
        {
            Id = 1,
            UserId = Guid.NewGuid().ToString(),
            WorkshopId = Guid.NewGuid(),
        };

        var application = ApplicationGenerator.Generate().WithParent(ParentGenerator.Generate());

        var favorites = new List<Favorite>() { favorite };
        var applications = new List<Application>() { application };

        var recipientsIds = new List<string>()
        {
            favorite.UserId,
            application.Parent.UserId,
        };

        var workshop = WorkshopGenerator.Generate();

        workshopService.Setup(x => x.GetById(workshop.Id, It.IsAny<bool>())).ReturnsAsync(mapper.Map<WorkshopDto>(workshop));
        favoriteRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Favorite, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Favorite, object>>, SortDirection>>(),
                It.IsAny<bool>())).Returns(favorites.AsTestAsyncEnumerableQuery());

        applicationRepository.Setup(x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>())).Returns(applications.AsTestAsyncEnumerableQuery());

        // Act
        await service.Delete(workshop.Id).ConfigureAwait(false);

        // Assert
        notificationServiceMock.Verify(
            x => x.Create(
                NotificationType.Workshop,
                NotificationAction.Delete,
                workshop.Id,
                recipientsIds,
                It.Is<Dictionary<string, string>>(c => c.ContainsKey(titleKey) && c[titleKey] == workshop.Title),
                null),
            Times.Once);
    }
}