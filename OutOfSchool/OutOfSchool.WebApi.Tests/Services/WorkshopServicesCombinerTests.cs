using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.Mapping;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class WorkshopServicesCombinerTests
{
    private Mock<IWorkshopService> workshopService;
    private IMapper mapper;
    private Mock<INotificationService> notificationServiceMock;
    private Mock<IEntityRepositorySoftDeleted<long, Favorite>> favoriteRepository;
    private Mock<IApplicationRepository> applicationRepository;

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
        var esProvider = new Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>>();

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

        var favorites = new List<Favorite>();
        favorites.Add(favorite);

        var applications = new List<Application>();
        applications.Add(application);

        var recipientsIds = new List<string>();
        recipientsIds.Add(favorite.UserId);
        recipientsIds.Add(application.Parent.UserId);

        var workshop = WorkshopGenerator.Generate();

        workshopService.Setup(x => x.GetById(workshop.Id)).ReturnsAsync(mapper.Map<WorkshopDto>(workshop));
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
