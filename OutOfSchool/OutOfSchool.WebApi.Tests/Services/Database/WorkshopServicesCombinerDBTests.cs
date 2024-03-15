using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.Mapping;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class WorkshopServicesCombinerDBTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private OutOfSchoolDbContext dbContext;

    private Mock<IWorkshopService> workshopService;
    private Mock<IElasticsearchSynchronizationService> elasticsearchSynchronizationService;
    private IMapper mapper;
    private Mock<IOptions<NotificationsConfig>> notificationsConfig;

    private IWorkshopServicesCombiner service;

    [SetUp]
    public void SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        dbContext = new OutOfSchoolDbContext(dbContextOptions);

        workshopService = new Mock<IWorkshopService>();
        elasticsearchSynchronizationService = new Mock<IElasticsearchSynchronizationService>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();

        var favoriteRepository = new EntityRepositorySoftDeleted<long, Favorite>(dbContext);
        var applicationRepository = new ApplicationRepository(dbContext);
        var workshopStrategy = new Mock<IWorkshopStrategy>();
        var currentUserService = new Mock<ICurrentUserService>();
        var ministryAdminService = new Mock<IMinistryAdminService>();
        var regionAdminService = new Mock<IRegionAdminService>();
        var codeficatorService = new Mock<ICodeficatorService>();
        var esProvider = new Mock<IElasticsearchProvider<WorkshopES, WorkshopFilterES>>();

        notificationsConfig = new Mock<IOptions<NotificationsConfig>>();

        var notificationRepository = new NotificationRepository(dbContext);
        var loggerNotificationService = new Mock<ILogger<NotificationService>>();
        var stringLocalizer = new Mock<IStringLocalizer<SharedResource>>();

        var notificationsHub = new Mock<IHubContext<NotificationHub>>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();

        notificationsHub.Setup(wh => wh.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(client => client.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

        var notificationService = new NotificationService(
            notificationRepository,
            loggerNotificationService.Object,
            stringLocalizer.Object,
            mapper,
            notificationsHub.Object,
            notificationsConfig.Object);

        service = new WorkshopServicesCombiner(
            workshopService.Object,
            elasticsearchSynchronizationService.Object,
            notificationService,
            favoriteRepository,
            applicationRepository,
            workshopStrategy.Object,
            currentUserService.Object,
            ministryAdminService.Object,
            regionAdminService.Object,
            codeficatorService.Object,
            esProvider.Object,
            mapper);

        Seed();
    }

    [TearDown]
    public void Dispose()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task Delete_WhenCalled_CreateNotificationWithTitleInAdditionalData()
    {
        // Arrange
        string titleKey = "Title";

        var workshop = WorkshopGenerator.Generate().WithApplications();
        workshop.Applications[0].Status = ApplicationStatus.Approved;
        dbContext.Add(workshop);
        await dbContext.SaveChangesAsync();

        workshopService.Setup(x => x.GetById(workshop.Id)).ReturnsAsync(mapper.Map<WorkshopDto>(workshop));
        workshopService.Setup(x => x.Delete(workshop.Id)).Returns(Task.CompletedTask);
        elasticsearchSynchronizationService.Setup(x => x.AddNewRecordToElasticsearchSynchronizationTable(
                ElasticsearchSyncEntity.Workshop,
                workshop.Id,
                ElasticsearchSyncOperation.Delete))
            .Returns(Task.CompletedTask);

        notificationsConfig
            .SetupGet(c => c.Value)
            .Returns(new NotificationsConfig() { Enabled = true });

        // Act
        await service.Delete(workshop.Id).ConfigureAwait(false);

        // Assert
        Notification result = await dbContext.Notifications.FirstOrDefaultAsync(x => x.ObjectId == workshop.Id).ConfigureAwait(false);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Data.ContainsKey(titleKey));
        Assert.AreEqual(result.Data[titleKey], workshop.Title);
    }

    private void Seed()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
}
