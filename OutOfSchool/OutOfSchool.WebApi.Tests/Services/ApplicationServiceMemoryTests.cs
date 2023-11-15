using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Config;
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
        var application = Applications();
        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);

        // Act
        var result = await service.GetAll(new ApplicationFilter());

        // Assert
        Assert.AreEqual(result.Entities.Count, application.Count);
    }

    [Test]
    public async Task GetApplications_WhenCalled_ShouldReturnBlockedApplications()
    {
        // Arrange
        var application = Applications().Where(a => a.IsBlocked).ToList();
        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);

        // Act
        var result = await service.GetAll(new ApplicationFilter() { ShowBlocked = true });

        // Assert
        Assert.AreEqual(result.Entities.Count, application.Count);
    }

    [Test]
    public async Task GetApplications_WhenCalled_ShouldReturnUnblockedApplications()
    {
        // Arrange
        var application = Applications().Where(a => !a.IsBlocked).ToList();
        currentUserServiceMock.Setup(c => c.IsAdmin()).Returns(true);
        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(false);
        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(false);

        // Act
        var result = await service.GetAll(new ApplicationFilter() { ShowBlocked = false });

        // Assert
        Assert.AreEqual(result.Entities.Count, application.Count);
    }

    private void SeedDatabase()
    {
        using var ctx = new OutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            ctx.Applications.AddRange(Applications());

            ctx.SaveChanges();
        }
    }

    private List<Application> Applications()
    {
        var workshops = WorkshopGenerator.Generate(3);
        var children = ChildGenerator.Generate(3);

        return new List<Application>()
        {
            new Application()
            {
                Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8db"),
                IsBlocked = false,
                Status = ApplicationStatus.Pending,
                WorkshopId = workshops[0].Id,
                ChildId = children[0].Id,
                Parent = new Parent
                {
                    User = new User
                    {
                        Email = "sea@gmail.com",
                        FirstName = "Арієль",
                        Id = "08da847c-2f6d-4327-8184-b1a11c8f7008",
                        LastName = "Русалонька",
                        MiddleName = "Моряка",
                        PhoneNumber = "498344943",
                    },
                    Gender = Gender.Female,
                    DateOfBirth = DateTime.Now,
                    UserId = "06da847c-2f6d-4327-8184-b1a11c8f7008",
                    Id = new Guid("05da847c-2f6d-4327-8184-b1a11c8f7008"),
                },
                ParentId = new Guid("05da847c-2f6d-4327-8184-b1a11c8f7008"),
                Workshop = workshops[0],
                Child = children[0],
            },
            new Application()
            {
                Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be"),
                IsBlocked = false,
                Status = ApplicationStatus.Rejected,
                WorkshopId = workshops[1].Id,
                ChildId = children[1].Id,
                Parent = new Parent
                {
                    User = new User
                    {
                        Email = "sea@gmail.com",
                        FirstName = "Арієль",
                        Id = "09da847c-2f6d-5327-8184-b1a11c8f7808",
                        LastName = "Русалонька",
                        MiddleName = "Моряка",
                        PhoneNumber = "498344943",
                    },
                    Gender = Gender.Female,
                    DateOfBirth = DateTime.Now,
                    UserId = "09da847c-2f6d-5327-8184-b1a11c8f7808",
                    Id = new Guid("09da847c-2f6d-5327-8184-b1a11c8f7808"),
                },
                ParentId = new Guid("09da847c-2f6d-5327-8184-b1a11c8f7808"),
                Workshop = workshops[1],
                Child = children[1],
            },
            new Application()
            {
                Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"),
                IsBlocked = true,
                Status = ApplicationStatus.Pending,
                WorkshopId = workshops[2].Id,
                ChildId = children[2].Id,
                Parent = new Parent
                {
                    User = new User
                    {
                        Email = "sea@gmail.com",
                        FirstName = "Арієль",
                        Id = "06da847c-2f6d-4327-8184-b1a11c8f7008",
                        LastName = "Русалонька",
                        MiddleName = "Моряка",
                        PhoneNumber = "498344943",
                    },
                    Gender = Gender.Female,
                    DateOfBirth = DateTime.Now,
                    UserId = "06da847c-2f6d-4327-8184-b1a11c8f7008",
                    Id = new Guid("02da847c-2f6d-4327-8184-b1a11c8f7008"),
                },
                ParentId = new Guid("02da847c-2f6d-4327-8184-b1a11c8f7008"),
                Workshop = workshops[2],
                Child = children[2],
            },
        };
    }
}
