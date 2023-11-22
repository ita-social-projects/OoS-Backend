using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Elastic.CommonSchema;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Signers;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ApplicationServiceMemoryTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context; 
    private IApplicationService service;
    private IApplicationRepository applicationRepository;
    private Mock<IWorkshopRepository> workshopRepositoryMock;
    private Mock<ILogger<ApplicationService>> logger;
    private IMapper mapper;
    private Mock<INotificationService> notificationService;
    private Mock<IProviderAdminService> providerAdminService;
    private Mock<IChangesLogService> changesLogService;
    private Mock<IWorkshopServicesCombiner> workshopServiceCombinerMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IAreaAdminService> areaAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;

    private Mock<IOptions<ApplicationsConstraintsConfig>> applicationsConstraintsConfig;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new OutOfSchoolDbContext(options);

        applicationRepository = new ApplicationRepository(context);
        workshopRepositoryMock = new Mock<IWorkshopRepository>();
        notificationService = new Mock<INotificationService>();
        providerAdminService = new Mock<IProviderAdminService>();
        changesLogService = new Mock<IChangesLogService>();
        workshopServiceCombinerMock = new Mock<IWorkshopServicesCombiner>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();

        logger = new Mock<ILogger<ApplicationService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();

        applicationsConstraintsConfig = new Mock<IOptions<ApplicationsConstraintsConfig>>();
        applicationsConstraintsConfig.Setup(x => x.Value)
            .Returns(new ApplicationsConstraintsConfig()
            {
                ApplicationsLimit = 2,
                ApplicationsLimitDays = 7,
            });

        service = new ApplicationService(
            applicationRepository,
            logger.Object,
            workshopRepositoryMock.Object,
            mapper,
            applicationsConstraintsConfig.Object,
            notificationService.Object,
            providerAdminService.Object,
            changesLogService.Object,
            workshopServiceCombinerMock.Object,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            regionAdminServiceMock.Object,
            areaAdminServiceMock.Object,
            codeficatorServiceMock.Object);

        SeedDatabase();
    }

    [Test]
    public async Task GetApplications_WhenCalled_ShouldReturnAllApplications()
    {
        // Arrange
        var application = GetAllFromMemory();
        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);

        // Act
        var result = await service.GetAll(new ApplicationFilter() { Show = ShowApplications.All });

        // Assert
        Assert.AreEqual(result.Entities.Count, application.Count);
    }

    [Test]
    public async Task GetApplications_WhenCalled_ShouldReturnBlockedApplications()
    {
        // Arrange
        var application = GetAllFromMemory().Where(a => a.IsBlocked).ToList();
        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);

        // Act
        var result = await service.GetAll(new ApplicationFilter() { Show = ShowApplications.Blocked });

        // Assert
        Assert.AreEqual(result.Entities.Count, application.Count);
        Assert.IsTrue(result.Entities.All(a => a.IsBlocked));
    }

    [Test]
    public async Task GetApplications_WhenCalled_ShouldReturnUnblockedApplications()
    {
        // Arrange
        var application = GetAllFromMemory().Where(a => !a.IsBlocked).ToList();
        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);

        // Act
        var result = await service.GetAll(new ApplicationFilter() { Show = ShowApplications.Unblocked });

        // Assert
        Assert.AreEqual(result.Entities.Count, application.Count);
        Assert.IsTrue(result.Entities.All(a => !a.IsBlocked));
    }

    private void SeedDatabase()
    {
        using var ctx = new OutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            var workshop = WorkshopGenerator.Generate();
            ctx.Workshops.Add(workshop);

            var child = ChildGenerator.Generate();
            ctx.Children.Add(child);

            var user = UserGenerator.Generate();
            ctx.Users.Add(user);

            var parent = ParentGenerator.Generate();
            parent.UserId = user.Id;
            ctx.Parents.Add(parent);

            var applications = new List<Application>()
            {
                new Application()
                {
                    Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8db"),
                    IsBlocked = false,
                    Status = ApplicationStatus.Pending,
                    WorkshopId = workshop.Id,
                    ChildId = child.Id,
                    ParentId = parent.Id,
                },
                new Application()
                {
                    Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be"),
                    IsBlocked = false,
                    Status = ApplicationStatus.Rejected,
                    WorkshopId = workshop.Id,
                    ChildId = child.Id,
                    ParentId = parent.Id,
                },
                new Application()
                {
                    Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
                    IsBlocked = true,
                    Status = ApplicationStatus.Pending,
                    WorkshopId = workshop.Id,
                    ChildId = child.Id,
                    ParentId = parent.Id,
                },
            };

            ctx.Applications.AddRange(applications);

            ctx.SaveChanges();
        }
    }

    private List<Application> GetAllFromMemory()
    {
        using var ctx = new OutOfSchoolDbContext(options);
        {
            return ctx.Applications.ToList();
        }
    }
}
