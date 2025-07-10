using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.SubordinationStructure;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.Common.Config;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
[SingleThreaded]
public class SensitiveWorkshopsServiceDBTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private TestOutOfSchoolDbContext dbContext;

    private ISensitiveWorkshopsService sensitiveWorkshopService;
    private IWorkshopRepository workshopRepository;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<IInstitutionHierarchyService>  institutionHierarchyServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ILanguageService> languageServiceMock;
    private Mock<ITagService> tagServiceMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IEntityRepository<long, Tag>> tagRepository;
    private Mock<IContactsService<Workshop, IHasContactsDto<Workshop>>> contactsServiceMock;
    private Mock<IApplicationRepository> applicationRepositoryMock;
    private Mock<IFeatureManager> featureManagerMock;
    private Mock<IChangesLogService> changesLogServiceMock;
    private Mock<IOptions<InstitutionOptions>> institutionOptionsMock;
    [SetUp]
    public void SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseLazyLoadingProxies()
            .Options;

        dbContext = new TestOutOfSchoolDbContext(dbContextOptions);

        workshopRepository = new WorkshopRepository(dbContext);
        institutionHierarchyServiceMock = new Mock<IInstitutionHierarchyService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        languageServiceMock = new Mock<ILanguageService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        tagServiceMock = new Mock<ITagService>();
        tagRepository = new Mock<IEntityRepository<long, Tag>>();
        contactsServiceMock = new Mock<IContactsService<Workshop, IHasContactsDto<Workshop>>>();
        applicationRepositoryMock = new Mock<IApplicationRepository>();
        featureManagerMock = new Mock<IFeatureManager>();
        searchStringServiceMock = new Mock<ISearchStringService>();
        changesLogServiceMock = new Mock<IChangesLogService>();
        institutionOptionsMock = new  Mock<IOptions<InstitutionOptions>>();
        sensitiveWorkshopService =
            new WorkshopService(
                workshopRepository,
                languageServiceMock.Object,
                institutionHierarchyServiceMock.Object,
                tagRepository.Object,
                new Mock<IEntityRepositorySoftDeleted<long, DateTimeRange>>().Object,
                new Mock<IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop>>().Object,
                new Mock<ITeacherService>().Object,
                new Mock<ILogger<WorkshopService>>().Object,
                new Mock<IImageDependentEntityImagesInteractionService<Workshop>>().Object,
                new Mock<IAverageRatingService>().Object,
                new Mock<IProviderRepository>().Object,
                currentUserServiceMock.Object,
                ministryAdminServiceMock.Object,
                regionAdminServiceMock.Object,
                codeficatorServiceMock.Object,
                tagServiceMock.Object,
                searchStringServiceMock.Object,
                contactsServiceMock.Object,
                applicationRepositoryMock.Object,
                featureManagerMock.Object,
                changesLogServiceMock.Object,
                institutionOptionsMock.Object);

        languageServiceMock.Setup(x => x.GetById(It.Is<long>(id => id == 1)))
                .ReturnsAsync(new LanguageDto { Id = 1, Name = "English" });
        institutionOptionsMock.Setup(x => x.Value)
            .Returns(new InstitutionOptions { MinistryOfSportTitle = "Мінспорт" });
        
        MockInstitutionHierarchy();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
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
        var language = new Language
        {
            Id = 1,
            Code = "en",
            Name = "English"
        };

        dbContext.Attach(language);
        await dbContext.SaveChangesAsync();

        var catottgCache = new Dictionary<long, CATOTTG>();
        var workshops = new List<Workshop>();

        var configs = GetWorkshopConfigs();

        foreach (var config in configs)
        {
            if (!catottgCache.TryGetValue(config.CATOTTGId, out var catottg))
            {
                catottg = CATOTTGGenerator.Generate();
                catottg.Id = config.CATOTTGId;
                catottgCache[catottg.Id] = catottg;
            }

            var institutionHierarchy = InstitutionHierarchyGenerator.Generate();
            institutionHierarchy.InstitutionId = config.InstitutionId;

            var address = new ContactsAddress
            {
                CATOTTGId = catottg.Id,
                BuildingNumber = config.Building,
                Street = config.Street,
                CATOTTG = catottg
            };

            var workshop = WorkshopGenerator.Generate()
                .WithAddress(address)
                .WithInstitutionHierarchy(institutionHierarchy)
                .WithTeachers();

            workshop.Title = config.Title;
            workshop.ShortTitle = config.ShortTitle;
            workshop.LanguageOfEducationId = language.Id;
            workshop.LanguageOfEducation = language;
            workshop.ProviderTitle = config.ProviderTitle;
            workshop.ProviderTitleEn = config.ProviderTitleEn;
            workshop.Contacts.FirstOrDefault()?.Emails.Add(new()
            {
                Type = "Test",
                Address = config.Email
            });

            workshops.Add(workshop);
        }

        await dbContext.AddRangeAsync(catottgCache.Values);
        await dbContext.AddRangeAsync(workshops);
        await dbContext.SaveChangesAsync();

        return workshops;
    }

    private List<WorkshopSeedConfig> GetWorkshopConfigs()
    {
        return new List<WorkshopSeedConfig>
        {
            new() { Title = "Воркшоп з ШІ", ShortTitle = "Воркшоп ІІ", ProviderTitle = "Інститут технологій", ProviderTitleEn = "University of Technology", Email = "ai_workshop@institute.com", InstitutionId = Guid.Parse("d85a3f07-8d7b-45b1-871d-23c5f4e34b92"), CATOTTGId = 1, Street = "Maple Avenue", Building = "101" },
            new() { Title = "Курс з науки про дані, воркшоп", ShortTitle = "Наука про дані", ProviderTitle = "УПН", ProviderTitleEn = "University of Applied Sciences", Email = "datascience@university.com", InstitutionId = Guid.Parse("a45f2766-1d63-4027-a9ba-b39b6351faeb"), CATOTTGId = 2, Street = "Oak Street", Building = "57" },
            new() { Title = "Вступ до машинного навчання", ShortTitle = "Воркшоп даніL", ProviderTitle = "Академія", ProviderTitleEn = "Artificial Intelligence Academy", Email = "ml_basics@aiacademy.com", InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935"), CATOTTGId = 3, Street = "Pine Crescent", Building = "230" },
            new() { Title = "живопису основи", ShortTitle = "Живопис", ProviderTitle = "Школа сучасного живопису", ProviderTitleEn = "Modern Painting", Email = "ml_basics@aiacademy.com", InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935"), CATOTTGId = 4, Street = "Elm Drive", Building = "12B" },
            new() { Title = "Основи кібербезпеки", ShortTitle = "Дані з Кібербезпеки", ProviderTitle = "Навчальний центр безпеки", ProviderTitleEn = "Security Training Center", Email = "cybersecurity@trainingcenter.com", InstitutionId = Guid.Parse("4c865c12-e99a-456b-82b1-7a7c3a2d6935"), CATOTTGId = 5, Street = "Birch Lane", Building = "78" },
            new() { Title = "Просунутий Python-програмування", ShortTitle = "Python Pro", ProviderTitle = "Школа програмування", ProviderTitleEn = "Programming School", Email = "python_pro@programming.com", InstitutionId = Guid.Parse("b87f4f9e-453e-4c25-bdc3-ff85425f6b92"), CATOTTGId = 6, Street = "Cedar Road", Building = "5A" },
            new() { Title = "Історія", ShortTitle = "Pro", ProviderTitle = "Школа", ProviderTitleEn = "School", Email = "writingclub@historyexplorers.com", InstitutionId = Guid.Parse("b87f4f9e-453e-4c25-bdc3-ff85425f6b92"), CATOTTGId = 7, Street = "Cherry Street", Building = "349" },
            new() { Title = "Сучасна література", ShortTitle = "Python Pro", ProviderTitle = "Гурток творчого письма", ProviderTitleEn = "Creative Writing Club", Email = "fantasymyths@writingclub.com", InstitutionId = Guid.Parse("ed1dbb6e-22f7-4e69-8e75-41b9fd489dbd"), CATOTTGId = 8, Street = "Willow Boulevard", Building = "92" },
            new() { Title = "Сучасна музика", ShortTitle = "Python Pro", ProviderTitle = "Школа народної музики", ProviderTitleEn = "Folk Music School", Email = "worldmelodies@writingclub.com", InstitutionId = Guid.Parse("437fba48-99de-4f85-b4b3-73a098e4e1cc"), CATOTTGId = 9, Street = "Sycamore Court", Building = "17" },
        };
    }

    private void MockInstitutionHierarchy()
    {
        institutionHierarchyServiceMock.Setup(s => s.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(new InstitutionHierarchyDto
            {
                Institution = new InstitutionDto { Title = "Мінспорт"}
            });
    }
    private async Task<List<WorkshopDto>> MapWorkshopsToDtos()
    {
        var workshops = await SeedWorkshops();
        return workshops.ToDto();
    }
    private class WorkshopSeedConfig
    {
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public string ProviderTitle { get; set; }
        public string ProviderTitleEn { get; set; }
        public string Email { get; set; }
        public Guid InstitutionId { get; set; }
        public int CATOTTGId { get; set; }
        public string Street { get; set; }
        public string Building { get; set; }
        public long LanguageOfEducationId { get; set; } = 1;
    }
}
