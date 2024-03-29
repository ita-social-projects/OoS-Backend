using System.Collections.Generic;
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

    private IWorkshopServicesCombiner service;

    [SetUp]
    public void SetUp()
    {
        workshopService = new Mock<IWorkshopService>();
        var elasticsearchSynchronizationService = new Mock<IElasticsearchSynchronizationService>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();

        var favoriteRepository = new Mock<IEntityRepositorySoftDeleted<long, Favorite>>();
        var applicationRepository = new Mock<IApplicationRepository>();
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

        var workshop = WorkshopGenerator.Generate();

        workshopService.Setup(x => x.GetById(workshop.Id)).ReturnsAsync(mapper.Map<WorkshopDto>(workshop));

        // Act
        await service.Delete(workshop.Id).ConfigureAwait(false);

        // Assert
        notificationServiceMock.Verify(
            x => x.Create(
                NotificationType.Workshop,
                NotificationAction.Delete,
                workshop.Id,
                service as INotificationReciever,
                It.Is<Dictionary<string, string>>(c => c.ContainsKey(titleKey) && c[titleKey] == workshop.Title),
                null),
            Times.Once);
    }
}
