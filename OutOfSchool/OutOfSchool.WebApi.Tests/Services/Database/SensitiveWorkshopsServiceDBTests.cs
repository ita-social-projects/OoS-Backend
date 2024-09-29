using System;
using System.Collections.Generic;
using System.Linq;
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
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
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
    private IMapper mapper;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ITagService> tagServiceMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IEntityRepository<long, Tag>> tagRepository;

    [SetUp]
    public void SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseLazyLoadingProxies()
            .Options;

        dbContext = new OutOfSchoolDbContext(dbContextOptions);

        workshopRepository = new WorkshopRepository(dbContext);
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        tagServiceMock = new Mock<ITagService>();
        tagRepository = new Mock<IEntityRepository<long, Tag>>();

        searchStringServiceMock = new Mock<ISearchStringService>();
        sensitiveWorkshopService =
            new WorkshopService(
                workshopRepository,
                tagRepository.Object,
                new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>().Object,
                new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>().Object,
                new Mock<ITeacherService>().Object,
                new Mock<ILogger<WorkshopService>>().Object,
                mapper,
                new Mock<IImageDependentEntityImagesInteractionService<Workshop>>().Object,
                new Mock<IEmployeeRepository>().Object,
                new Mock<IAverageRatingService>().Object,
                new Mock<IProviderRepository>().Object,
                currentUserServiceMock.Object,
                ministryAdminServiceMock.Object,
                regionAdminServiceMock.Object,
                codeficatorServiceMock.Object,
                tagServiceMock.Object,
                searchStringServiceMock.Object);

        Seed();
    }

    [TearDown]
    public void Dispose()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilterIsNull_ShouldReturnDefaultAmountEntities()
    {
        // Arrange
        var workshopDtos = await MapWorkshopsToDtos();
        var filter = new WorkshopFilterAdministration();
        var entities = workshopDtos.
            Skip(filter.From).
            Take(filter.Size).
            ToList();

        var expectedResult = new SearchResult<WorkshopDto>()
        {
            TotalAmount = workshopDtos.Count,
            Entities = entities,
        };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins()
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task FetchByFilterForAdmins_ByFilterCriteria_ShouldBuildPredicateAndReturnMatchEntities()
    {
        // Arrange
        var workshopDtos = await MapWorkshopsToDtos();
        var filter = new WorkshopFilterAdministration()
        {
            InstitutionId = Guid.Parse("d85a3f07-8d7b-45b1-871d-23c5f4e34b92"),
            SearchString = "ворк  ,    ",
            Size = 100,
            CATOTTGId = 1,
        };

        codeficatorServiceMock.Setup(c => c.GetAllChildrenIdsByParentIdAsync(
            It.Is<long>(c => c == filter.CATOTTGId))).ReturnsAsync(new List<long>() { 1, 2, 3 });

        searchStringServiceMock.Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(["ворк"]);

        var expectedEntities = new List<WorkshopDto> { workshopDtos[0] };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        // Assert
        result.Entities.Should()
            .BeEquivalentTo(expectedEntities);
    }

    [Test]
    public async Task FetchByFilterForAdmins_SearchStringIsWhiteSpace_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var workshopDtos = await MapWorkshopsToDtos();
        var filter = new WorkshopFilterAdministration()
        {
            SearchString = "   ",
            Size = 9,
        };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        // Assert
        result.Entities.Should()
            .BeEquivalentTo(workshopDtos);
    }

    [Test]
    public async Task FetchByFilterForAdmins_SearchStringIsWitheSpaceWithSeparatorСomma_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var workshopDtos = await MapWorkshopsToDtos();
        var filter = new WorkshopFilterAdministration()
        {
            SearchString = " ,   ,   ,  ",
            Size = 10,
        };

        var expectedResult = new SearchResult<WorkshopDto>()
        {
            TotalAmount = workshopDtos.Count,
            Entities = workshopDtos,
        };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task FetchByFilterForAdmins_RoleMinistryAdmin_ShouldBuildPredicateAndReturnMatchEntities()
    {
        // Arrange
        var workshopDtos = await MapWorkshopsToDtos();
        var userId = Guid.NewGuid().ToString();
        var expectedList = new List<WorkshopDto>() { workshopDtos[3], workshopDtos[4] };
        var admin = new MinistryAdminDto() { InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935") };
        var filter = new WorkshopFilterAdministration() { SearchString = " ,  основ  ,   " };
        SetupMinistryAdminRole(userId, admin, filter, ["основ"]);
        var expectedResult = new SearchResult<WorkshopDto>()
        {
            TotalAmount = expectedList.Count,
            Entities = expectedList,
        };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilteringByCATOTTGIds_ShouldBuildPredicateAndReturnMatchEntities()
    {
        // Arrange
        var workshopDto = await MapWorkshopsToDtos();
        var userId = Guid.NewGuid().ToString();
        var expectedList = new List<WorkshopDto>() { workshopDto[2], workshopDto[3], workshopDto[4] };
        var admin = new RegionAdminDto() { CATOTTGId = 4, InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935") };
        SetupRegionAdminRole(userId, admin);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins()
            .ConfigureAwait(false);

        result.Entities.Should()
            .BeEquivalentTo(expectedList);
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilteringByProviderTitleEn_ShouldBuildPredicateAndReturnMatchEntities()
    {
        // Arrange
        var workshopDto = await MapWorkshopsToDtos();

        var expectedList = new List<WorkshopDto>() { workshopDto[0], workshopDto[1] };
        var filter = new WorkshopFilterAdministration() { SearchString = "workshop ,  univers  " };
        searchStringServiceMock.Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(["workshop", "univers"]);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        result.Entities.Should()
            .BeEquivalentTo(expectedList);
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilteringByEmail_ShouldBuildPredicateAndReturnMatchEntities()
    {
        // Arrange
        var workshopDto = await MapWorkshopsToDtos();
        var expectedList = new List<WorkshopDto>() { workshopDto[6], workshopDto[7], workshopDto[8] };
        var filter = new WorkshopFilterAdministration() { SearchString = "writingclub, test  " };
        searchStringServiceMock.Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(["writingclub", "test"]);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        result.Entities.Should()
            .BeEquivalentTo(expectedList);
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilteringByShortTitle_ShouldBuildPredicateAndReturnMatchEntities()
    {
        // Arrange
        var workshopDto = await MapWorkshopsToDtos();
        var expectedList = new List<WorkshopDto>() { workshopDto[1], workshopDto[2], workshopDto[4] };
        var filter = new WorkshopFilterAdministration() { SearchString = "ДАНІ  " };
        searchStringServiceMock.Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(["ДАНІ"]);

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        result.Entities.Should()
            .BeEquivalentTo(expectedList);
    }

    [Test]
    public async Task FetchByFilterForAdmins_RoleMinistryAdminFilterByInvalidInstitutionId_ShouldReturnEmptyList()
    {
        // Arrange
        await MapWorkshopsToDtos();
        var expectedList = new List<WorkshopDto>();
        var userId = Guid.NewGuid().ToString();
        var ministryAdmin = new MinistryAdminDto()
        {
            Id = userId,
            InstitutionId = Guid.Parse("d85a3f07-8d7b-45b1-871d-23c5f4e34b92"),
        };

        SetupMinistryAdminRole(userId, ministryAdmin);

        var filter = new WorkshopFilterAdministration()
        {
            InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935"),
        };

        var expectedResult = new SearchResult<WorkshopDto>()
        {
            TotalAmount = expectedList.Count,
            Entities = expectedList,
        };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        result.Should()
            .BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task FetchByFilterForAdmins_RoleRegionAdminFilterByInvalidInstitutionId_ShouldReturnEmptyList()
    {
        // Arrange
        await MapWorkshopsToDtos();
        var expectedList = new List<WorkshopDto>();
        var userId = Guid.NewGuid().ToString();

        var regionAdmin = new RegionAdminDto()
        {
            Id = userId,
            InstitutionId = Guid.Parse("d85a3f07-8d7b-45b1-871d-23c5f4e34b92"),
        };

        SetupRegionAdminRole(userId, regionAdmin);

        var filter = new WorkshopFilterAdministration()
        {
            InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935"),
        };

        var expectedResult = new SearchResult<WorkshopDto>()
        {
            TotalAmount = expectedList.Count,
            Entities = expectedList,
        };

        // Act
        var result = await sensitiveWorkshopService.FetchByFilterForAdmins(filter)
            .ConfigureAwait(false);

        result.Should()
            .BeEquivalentTo(expectedResult);
    }

    private void Seed()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    private void SetupMinistryAdminRole(
        string userId,
        MinistryAdminDto admin,
        WorkshopFilterAdministration filter = null,
        string[] splitedInput = null)
    {
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId.ToString());
        currentUserServiceMock.Setup(s => s.IsMinistryAdmin()).Returns(true);
        ministryAdminServiceMock.Setup(s => s.GetByUserId(userId.ToString())).ReturnsAsync(admin);
        searchStringServiceMock.Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
       .Returns(splitedInput);
    }

    private void SetupRegionAdminRole(string userId, RegionAdminDto admin)
    {
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId.ToString());
        currentUserServiceMock.Setup(s => s.IsRegionAdmin()).Returns(true);
        regionAdminServiceMock.Setup(s => s.GetByUserId(userId.ToString())).ReturnsAsync(admin);
        codeficatorServiceMock.Setup(c => c.GetAllChildrenIdsByParentIdAsync(
           It.Is<long>(id => id == admin.CATOTTGId))).ReturnsAsync(new List<long>() { 3, 4, 5 });
    }

    private async Task<List<Workshop>> SeedWorkshops()
    {
        var workshops = new List<Workshop>();

        var institutionHierarch = InstitutionHierarchyGenerator.Generate();
        var address = new Address() { CATOTTGId = 1, BuildingNumber = "101", Street = "Maple Avenue" };
        var workshop = WorkshopGenerator.Generate()
            .WithAddress(address)
            .WithInstitutionHierarchy(institutionHierarch)
            .WithTeachers();

        workshop.Title = "Воркшоп з ШІ";
        workshop.ShortTitle = "Воркшоп ІІ";
        workshop.ProviderTitle = "Інститут технологій";
        workshop.ProviderTitleEn = "University of Technology";
        workshop.Email = "ai_workshop@institute.com";
        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("d85a3f07-8d7b-45b1-871d-23c5f4e34b92");
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        address = new Address() { CATOTTGId = 2, BuildingNumber = "57", Street = "Oak Street" };
        workshop = WorkshopGenerator.Generate()
             .WithAddress(address)
             .WithInstitutionHierarchy(institutionHierarch)
             .WithTeachers();

        workshop.Title = "Курс з науки про дані, воркшоп";
        workshop.ShortTitle = "Наука про дані";
        workshop.ProviderTitle = "УПН";
        workshop.ProviderTitleEn = "University of Applied Sciences";
        workshop.Email = "datascience@university.com";
        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("a45f2766-1d63-4027-a9ba-b39b6351faeb");
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        address = new Address() { CATOTTGId = 3, BuildingNumber = "230", Street = "Pine Crescent" };
        workshop = WorkshopGenerator.Generate()
             .WithAddress(address)
             .WithInstitutionHierarchy(institutionHierarch)
             .WithTeachers();

        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935");
        workshop.Title = "Вступ до машинного навчання";
        workshop.ShortTitle = "Воркшоп даніL";
        workshop.ProviderTitle = "Академія";
        workshop.ProviderTitleEn = "Artificial Intelligence Academy";
        workshop.Email = "ml_basics@aiacademy.com";
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        address = new Address() { CATOTTGId = 4, BuildingNumber = "12B", Street = "Elm Drive" };
        workshop = WorkshopGenerator.Generate()
             .WithAddress(address)
             .WithInstitutionHierarchy(institutionHierarch)
             .WithTeachers();

        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935");
        workshop.Title = "живопису основи";
        workshop.ShortTitle = "Живопис";
        workshop.ProviderTitle = "Школа сучасного живопису";
        workshop.ProviderTitleEn = "Modern Painting";
        workshop.Email = "ml_basics@aiacademy.com";
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        address = new Address() { CATOTTGId = 5, BuildingNumber = "78", Street = "Birch Lane" };
        workshop = WorkshopGenerator.Generate()
            .WithAddress(address)
            .WithInstitutionHierarchy(institutionHierarch)
            .WithTeachers();

        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935");
        workshop.Title = "Основи кібербезпеки";
        workshop.ShortTitle = "Дані з Кібербезпеки";
        workshop.ProviderTitle = "Навчальний центр безпеки";
        workshop.ProviderTitleEn = "Security Training Center";
        workshop.Email = "cybersecurity@trainingcenter.com";
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        address = new Address() { CATOTTGId = 6, BuildingNumber = "5A", Street = "Cedar Road" };
        address.CATOTTGId = 6;
        workshop = WorkshopGenerator.Generate()
             .WithAddress(address)
             .WithInstitutionHierarchy(institutionHierarch)
             .WithTeachers();

        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("b87f4f9e-453e-4c25-bdc3-ff85425f6b92");
        workshop.Title = "Просунутий Python-програмування";
        workshop.ShortTitle = "Python Pro";
        workshop.ProviderTitle = "Школа програмування";
        workshop.ProviderTitleEn = "Programming School";
        workshop.Email = "python_pro@programming.com";
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        address = new Address() { CATOTTGId = 7, BuildingNumber = "349", Street = "Cherry Street" };
        workshop = WorkshopGenerator.Generate()
             .WithAddress(address)
             .WithInstitutionHierarchy(institutionHierarch)
             .WithTeachers();

        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("b87f4f9e-453e-4c25-bdc3-ff85425f6b92");
        workshop.Title = "Історія";
        workshop.ShortTitle = "Pro  ";
        workshop.ProviderTitle = "Школа";
        workshop.ProviderTitleEn = "School";
        workshop.Email = "writingclub@historyexplorers.com";
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        address = new Address() { CATOTTGId = 8, BuildingNumber = "92", Street = "Willow Boulevard" };
        workshop = WorkshopGenerator.Generate()
             .WithAddress(address)
             .WithInstitutionHierarchy(institutionHierarch)
             .WithTeachers();

        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("ed1dbb6e-22f7-4e69-8e75-41b9fd489dbd");
        workshop.Title = "Сучасна література";
        workshop.ShortTitle = "Python Pro";
        workshop.ProviderTitle = "Гурток творчого письма";
        workshop.ProviderTitleEn = "Creative Writing Club";
        workshop.Email = "fantasymyths@writingclub.com";
        workshops.Add(workshop);

        institutionHierarch = InstitutionHierarchyGenerator.Generate();
        address = new Address() { CATOTTGId = 9, BuildingNumber = "17", Street = "Sycamore Court" };
        workshop = WorkshopGenerator.Generate()
             .WithAddress(address)
             .WithInstitutionHierarchy(institutionHierarch)
             .WithTeachers();

        workshop.InstitutionHierarchy.InstitutionId = Guid.Parse("437fba48-99de-4f85-b4b3-73a098e4e1cc");
        workshop.Title = "Сучасна музика";
        workshop.ShortTitle = "Python Pro";
        workshop.ProviderTitle = "Школа народної музики";
        workshop.ProviderTitleEn = "Folk Music School";
        workshop.Email = "worldmelodies@writingclub.com";
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
}
