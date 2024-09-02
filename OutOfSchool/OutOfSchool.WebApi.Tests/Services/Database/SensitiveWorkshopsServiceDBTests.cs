using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class SensitiveWorkshopsServiceDBTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private OutOfSchoolDbContext dbContext;

    private ISensitiveWorkshopsService sensitiveWorkshopService;
    private IWorkshopRepository workshopRepository;
    private Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>> dateTimeRangeRepository;
    private Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>> roomRepository;
    private Mock<ITeacherService> teacherService;
    private Mock<ILogger<WorkshopService>> logger;
    private IMapper mapper;
    private Mock<IImageDependentEntityImagesInteractionService<Workshop>> workshopImagesMediator;
    private Mock<IProviderAdminRepository> providerAdminRepository;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IProviderRepository> providerRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Guid institutionId;

    [SetUp]
    public void SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .UseLazyLoadingProxies()
            .Options;

        dbContext = new OutOfSchoolDbContext(dbContextOptions);

        workshopRepository = new WorkshopRepository(dbContext);
        dateTimeRangeRepository = new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>();
        roomRepository = new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>();
        teacherService = new Mock<ITeacherService>();
        logger = new Mock<ILogger<WorkshopService>>();
        workshopImagesMediator = new Mock<IImageDependentEntityImagesInteractionService<Workshop>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        providerAdminRepository = new Mock<IProviderAdminRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        providerRepositoryMock = new Mock<IProviderRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();

        sensitiveWorkshopService =
            new WorkshopService(
                workshopRepository,
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
                codeficatorServiceMock.Object);

        institutionId = Guid.Parse("f72f6404-0d4a-4a6a-bee1-f1a660f0f556");
        Seed();
    }

    [TearDown]
    public void Dispose()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilterIsNull_ShouldBuildPredicateAndReturnAllEntities()
    {
        // Arrange
        var workshopDto = await MapWorkshopsToDtos();
        var expectedResult = new SearchResult<WorkshopDto>()
        {
            TotalAmount = workshopDto.Count,
            Entities = workshopDto,
        };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins().ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task FetchByFilterForAdmins_RoleRegionAdminByFilter_ShouldBuildPredicateAndReturnMatchEntities()
    {
        // Arrange
        var workshopDto = await MapWorkshopsToDtos();
        SetUpServices(new List<long> { 1, 2 }, 1);
        var filter = new WorkshopFilterAdministration() { SearchString = "keyWord" };
        var expectedEntities = new List<WorkshopDto> { workshopDto[0], workshopDto[1] };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter).ConfigureAwait(false);

        // Assert
        result.Entities.Should().BeEquivalentTo(expectedEntities);
    }

    private void Seed()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    private async Task<List<Workshop>> SeedWorkshops()
    {
        var workshops = new List<Workshop>();

        var institutionHierarch = InstitutionHierarchyGenerator.Generate();
        var workshop = WorkshopGenerator.Generate()
            .WithAddress()
            .WithInstitutionHierarchy(institutionHierarch);

        workshop.Address.CATOTTGId = 1;
        workshop.Keywords = "keyWord";
        workshop.InstitutionHierarchy.InstitutionId = institutionId;
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        workshop = WorkshopGenerator.Generate()
             .WithAddress()
             .WithInstitutionHierarchy(institutionHierarch);

        workshop.Address.CATOTTGId = 2;
        workshop.Keywords = "keyWord";
        workshop.InstitutionHierarchy.InstitutionId = institutionId;

        workshop.Address.CATOTTGId = 2;
        workshop.Keywords = "keyWord";
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        workshop = WorkshopGenerator.Generate()
            .WithAddress()
            .WithInstitutionHierarchy(institutionHierarch);

        workshop.Address.CATOTTGId = 3;
        workshop.Keywords = "keyWord";
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        workshop = WorkshopGenerator.Generate()
             .WithAddress()
             .WithInstitutionHierarchy(institutionHierarch);
        workshops.Add(workshop);

        await dbContext.AddRangeAsync(workshops);
        await dbContext.SaveChangesAsync();

        return workshops;
    }

    private async Task<List<WorkshopDto>> MapWorkshopsToDtos()
    {
        var workshops = await SeedWorkshops();
        return mapper.Map<List<WorkshopDto>>(workshops);
    }

    private void SetUpServices(IEnumerable<long> subSettlementsIds, long parentId)
    {
        var userId = Guid.NewGuid().ToString();
        var regionAdmin = new RegionAdminDto()
        {
            CATOTTGId = parentId,
            InstitutionId = institutionId,
        };
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        currentUserServiceMock.Setup(x => x.IsRegionAdmin()).Returns(true);
        regionAdminServiceMock.Setup(a => a.GetByUserId(userId)).ReturnsAsync(regionAdmin);
        codeficatorServiceMock.Setup(c => c.GetAllChildrenIdsByParentIdAsync(parentId)).ReturnsAsync(subSettlementsIds);
    }
}
