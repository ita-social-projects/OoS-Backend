using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Enums;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
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

    private IWorkshopServicesCombiner service;

    [SetUp]
    public void SetUp()
    {
        workshopService = new Mock<IWorkshopService>();
        var elasticsearchSynchronizationService = new Mock<IElasticsearchSynchronizationService>();
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

        workshopService.Setup(x => x.GetById(workshopDto.Id, false)).ReturnsAsync(workshopDto);
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

        workshopService.Setup(x => x.GetById(workshop.Id, false)).ReturnsAsync(mapper.Map<WorkshopDto>(workshop));
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
