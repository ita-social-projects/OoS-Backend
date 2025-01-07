using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class WorkshopServiceDBTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private TestOutOfSchoolDbContext dbContext;

    private IWorkshopService workshopService;
    private IWorkshopRepository workshopRepository;
    private Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>> dateTimeRangeRepository;
    private Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>> roomRepository;
    private Mock<ITeacherService> teacherService;
    private Mock<ILogger<WorkshopService>> logger;
    private IMapper mapper;
    private Mock<IImageDependentEntityImagesInteractionService<Workshop>> workshopImagesMediator;
    private Mock<IEmployeeRepository> providerAdminRepository;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IProviderRepository> providerRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<ITagService> tagServiceMock;
    private Mock<IEntityRepository<long, Tag>> tagRepository;
    private Mock<IContactsService<Workshop, IHasContactsDto<Workshop>>> contactsServiceMock;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .EnableSensitiveDataLogging()
            .Options;

        dbContext = new TestOutOfSchoolDbContext(dbContextOptions);

        workshopRepository = new WorkshopRepository(dbContext);
        dateTimeRangeRepository = new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>();
        roomRepository = new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>();
        teacherService = new Mock<ITeacherService>();
        logger = new Mock<ILogger<WorkshopService>>();
        workshopImagesMediator = new Mock<IImageDependentEntityImagesInteractionService<Workshop>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        providerAdminRepository = new Mock<IEmployeeRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        providerRepositoryMock = new Mock<IProviderRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        tagServiceMock = new Mock<ITagService>();
        var searchStringServiceMock = new Mock<ISearchStringService>();
        tagRepository = new Mock<IEntityRepository<long, Tag>>();
        contactsServiceMock = new Mock<IContactsService<Workshop, IHasContactsDto<Workshop>>>();

        workshopService =
                new WorkshopService(
                    workshopRepository,
                    tagRepository.Object,
                    dateTimeRangeRepository.Object,
                    roomRepository.Object,
                    teacherService.Object,
                    logger.Object,
                    mapper,
                    workshopImagesMediator.Object,
                    providerAdminRepository.Object,
                    averageRatingServiceMock.Object,
                    providerRepositoryMock.Object,
                    currentUserServiceMock.Object,
                    ministryAdminServiceMock.Object,
                    regionAdminServiceMock.Object,
                    codeficatorServiceMock.Object,
                    tagServiceMock.Object,
                    searchStringServiceMock.Object,
                    contactsServiceMock.Object);

        Seed();
    }

    [TearDown]
    public void Dispose()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task GetByFilter_WhenSetFormOfLearning_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        await SeedFormOfLearningWorkshops();

        var filter = new WorkshopFilter()
        {
            FormOfLearning = new List<FormOfLearning>()
            {
                FormOfLearning.Offline,
            },
        };

        // Act
        var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(2, result.TotalAmount);
    }

    [Test]
    public async Task GetByFilter_WhenSetFormOfLearning_ReturnNothing()
    {
        // Arrange
        await SeedFormOfLearningWorkshops();

        var filter = new WorkshopFilter()
        {
            FormOfLearning = new List<FormOfLearning>()
            {
                FormOfLearning.Mixed,
            },
        };

        // Act
        var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(0, result.TotalAmount);
    }

    [Test]
    public async Task GetByFilter_WhenNotSetFormOfLearning_ReturnAll()
    {
        // Arrange
        await SeedFormOfLearningWorkshops();

        var filter = new WorkshopFilter();

        // Act
        var result = await workshopService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(5, result.TotalAmount);
    }

    [Test]
    public async Task Exists_WhenSendWrongId_ReturnFalse()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate();
        dbContext.Add(workshop);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await workshopService.Exists(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(false, result);
    }

    [Test]
    public async Task Exists_WhenSendGoodId_ReturnTrue()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate();
        dbContext.Add(workshop);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await workshopService.Exists(workshop.Id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(true, result);
    }

    #region private

    private TestOutOfSchoolDbContext GetContext() => new TestOutOfSchoolDbContext(dbContextOptions);

    private IWorkshopRepository GetWorkshopRepository(TestOutOfSchoolDbContext dbContext)
        => new WorkshopRepository(dbContext);

    private void Seed()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    private Task SeedFormOfLearningWorkshops()
    {
        var workshops = new List<Workshop>();

        var workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Offline;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Offline;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Online;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Online;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        workshop = WorkshopGenerator.Generate().WithProvider();
        workshop.FormOfLearning = FormOfLearning.Online;
        workshop.Provider.Status = ProviderStatus.Approved;
        workshops.Add(workshop);

        dbContext.AddRange(workshops);
        return dbContext.SaveChangesAsync();
    }

    #endregion
}
