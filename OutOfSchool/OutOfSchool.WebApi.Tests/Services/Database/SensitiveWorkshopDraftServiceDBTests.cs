using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Common.Config;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.WorkshopDraftRepository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.BusinessLogic.Services.SubordinationStructure;
using OutOfSchool.SportsRegistryApiClient.Interfaces;

namespace OutOfSchool.WebApi.Tests.Services.Database;

[TestFixture]
public class SensitiveWorkshopDraftServiceDBTests
{
    private DbContextOptions<OutOfSchoolDbContext> dbContextOptions;
    private TestOutOfSchoolDbContext dbContext;

    private IReadOnlyCollection<WorkshopDraft> workshopDrafts;

    private ISensitiveWorkshopDraftService workshopDraftService;
    private IWorkshopDraftRepository workshopDraftRepository;

    private Mock<ISportsRegistryProviderService> sportsRegistryProviderServiceMock;
    private Mock<IProviderService> providerServiceMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IWorkshopServicesCombinerV2> workshopServiceCombinerV2Mock;
    private Mock<ILanguageService> languageServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IInstitutionHierarchyRepository> institutionHierarchyRepositoryMock;
    private Mock<ICodeficatorRepository> codeficatorRepositoryMock;
    private Mock<IChangesLogService> changesLogServiceMock;
    private Mock<IOptions<InstitutionOptions>> institutionOptionsMock;
    private Mock<IOptions<ImageStorageOptions>> imageStorageOptionsMock;
    private Mock<IInstitutionHierarchyService> institutionHierarchyServiceMock;
    private Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>> workshopDraftImagesServiceMock;

    [SetUp]
    public async Task SetUp()
    {
        dbContextOptions = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseLazyLoadingProxies()
            .Options;

        dbContext = new TestOutOfSchoolDbContext(dbContextOptions);
        workshopDraftRepository = new WorkshopDraftRepository(dbContext);
        institutionHierarchyServiceMock = new Mock<IInstitutionHierarchyService>();
        sportsRegistryProviderServiceMock = new Mock<ISportsRegistryProviderService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        providerServiceMock = new Mock<IProviderService>();
        workshopServiceCombinerV2Mock = new Mock<IWorkshopServicesCombinerV2>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        searchStringServiceMock = new Mock<ISearchStringService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        institutionHierarchyRepositoryMock = new Mock<IInstitutionHierarchyRepository>();
        codeficatorRepositoryMock = new Mock<ICodeficatorRepository>();
        languageServiceMock = new Mock<ILanguageService>();
        changesLogServiceMock = new Mock<IChangesLogService>();
        workshopDraftImagesServiceMock = new Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>>();

        var options = new Mock<IOptions<UploadConcurrencySettings>>();
        var settings = new UploadConcurrencySettings();
        options.Setup(o => o.Value).Returns(settings);

        var logger = new Mock<ILogger<WorkshopDraftService>>();
        var teacherDraftImagesService = new Mock<IEntityCoverImageInteractionService<TeacherDraft>>();
        institutionOptionsMock = new Mock<IOptions<InstitutionOptions>>();
        imageStorageOptionsMock = new Mock<IOptions<ImageStorageOptions>>();
        workshopDraftService = new WorkshopDraftService(
                   logger.Object,
                   sportsRegistryProviderServiceMock.Object,
                   languageServiceMock.Object,
                   workshopDraftRepository,
                   workshopDraftImagesServiceMock.Object,
                   providerServiceMock.Object,
                   currentUserServiceMock.Object,
                   teacherDraftImagesService.Object,
                   options.Object,
                   workshopServiceCombinerV2Mock.Object,
                   regionAdminServiceMock.Object,
                   ministryAdminServiceMock.Object,
                   codeficatorServiceMock.Object,
                   searchStringServiceMock.Object,
                   institutionHierarchyRepositoryMock.Object,
                   codeficatorRepositoryMock.Object,
                   changesLogServiceMock.Object,
                   institutionHierarchyServiceMock.Object,
                   institutionOptionsMock.Object,
                   imageStorageOptionsMock.Object);

        await Seed();
    }

    #region FetchByFilterForAdmins

    [Test]
    public async Task FetchByFilterForAdmins_WithDefaultFilter_SortsAsExpected()
    {
        // Arrange
        var filter = new WorkshopDraftFilterAdministration();
        searchStringServiceMock
            .Setup(s => s.SplitSearchString(It.IsAny<string>()))
            .Returns(Array.Empty<string>());
        currentUserServiceMock.Setup(s => s.IsMinistryAdmin())
            .Returns(false);
        currentUserServiceMock.Setup(s => s.IsRegionAdmin())
            .Returns(false);
        institutionHierarchyRepositoryMock.Setup(r => r.Get(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(), It.IsAny<Dictionary<Expression<Func<InstitutionHierarchy, object>>, SortDirection>>()))
            .Returns(new List<InstitutionHierarchy>().AsTestAsyncEnumerableQuery());
        codeficatorRepositoryMock.Setup(r => r.Get(It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<Expression<Func<CATOTTG, bool>>>(), It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsTestAsyncEnumerableQuery());

        await using var context = GetContext();
        var expected = context.WorkshopDrafts
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.DraftStatus == WorkshopDraftStatus.PendingModeration ? 0
                    : x.DraftStatus == WorkshopDraftStatus.EditedByModerator ? 1 : 2)
            .ThenBy(x => x.ModifiedAt)
            .ThenBy(x => x.Id)
            .ToList();

        // Act
        var result = await workshopDraftService.FetchByFilterForAdmins(filter).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Entities.Should().NotBeNull();
        result.TotalAmount.Should().Be(expected.Count);
        var resultList = result.Entities.ToList();
        for (int i = 0; i < expected.Count; i++)
        {
            resultList[i].WorkshopDraftId.Should().Be(expected[i].Id);
        }
    }

    #endregion

    private TestOutOfSchoolDbContext GetContext()
        => new TestOutOfSchoolDbContext(dbContextOptions);

    private async Task Seed()
    {
        await using var context = GetContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        var workshopDrafts = new List<WorkshopDraft>();

        var configs = GetWorkshopDraftConfigs();
        foreach (var config in configs)
        {
            var workshopV2Dto = WorkshopV2DtoGenerator.Generate();
            workshopV2Dto.Contacts = [
                new ContactsDto
                {
                    IsDefault = true,
                    Address = ContactsAddressDtoGenerator.Generate()
                }];
            var workshopDraft = workshopV2Dto.ToDraft();

            workshopDraft.WorkshopDraftContent.Title = config.Title;
            workshopDraft.WorkshopDraftContent.ShortTitle = config.ShortTitle;
            workshopDraft.Provider = ProvidersGenerator.Generate();
            workshopDraft.Provider.FullTitle = config.ProviderTitle;
            workshopDraft.Provider.FullTitleEn = config.ProviderTitleEn;
            workshopDraft.DraftStatus = config.WorkshopDraftStatus;

            workshopDrafts.Add(workshopDraft);
        }        

        context.AddRange(workshopDrafts);

        for (int i = 0; i < workshopDrafts.Count; i++)
        {
            context.Entry(workshopDrafts[i]).Property(nameof(WorkshopDraft.CreatedAt)).CurrentValue = configs[i].CreatedAt;
            context.Entry(workshopDrafts[i]).Property(nameof(WorkshopDraft.ModifiedAt)).CurrentValue = configs[i].ModifiedAt;
        }

        await context.SaveChangesAsync();
    }

    private List<WorkshopDraftSeedConfig> GetWorkshopDraftConfigs()
    {
        var baseDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);

        return new List<WorkshopDraftSeedConfig>
        {
            new() { Title = "Воркшоп з ШІ", ShortTitle = "Воркшоп ІІ", ProviderTitle = "Інститут технологій", ProviderTitleEn = "University of Technology", WorkshopDraftStatus = WorkshopDraftStatus.PendingModeration, CreatedAt = baseDate.AddDays(0),ModifiedAt = baseDate.AddDays(0).AddHours(2)},
            new() {Title = "Курс з науки про дані, воркшоп", ShortTitle = "Наука про дані", ProviderTitle = "УПН", ProviderTitleEn = "University of Applied Sciences", WorkshopDraftStatus = WorkshopDraftStatus.EditedByModerator, CreatedAt = baseDate.AddDays(1),ModifiedAt = baseDate.AddDays(1).AddHours(2)},
            new() {Title = "Вступ до машинного навчання", ShortTitle = "Воркшоп даніL", ProviderTitle = "Академія", ProviderTitleEn = "Artificial Intelligence Academy", WorkshopDraftStatus = WorkshopDraftStatus.EditedByModerator, CreatedAt = baseDate.AddDays(2),ModifiedAt = baseDate.AddDays(2).AddHours(2)},
            new() {Title = "живопису основи", ShortTitle = "Живопис", ProviderTitle = "Школа сучасного живопису", ProviderTitleEn = "Modern Painting", WorkshopDraftStatus = WorkshopDraftStatus.PendingModeration, CreatedAt = baseDate.AddDays(3),ModifiedAt = baseDate.AddDays(3).AddHours(2)},
            new() {Title = "Основи кібербезпеки", ShortTitle = "Дані з Кібербезпеки", ProviderTitle = "Навчальний центр безпеки", ProviderTitleEn = "Security Training Center", WorkshopDraftStatus = WorkshopDraftStatus.EditedByModerator, CreatedAt = baseDate.AddDays(4),ModifiedAt = baseDate.AddDays(4).AddHours(2)},
            new() {Title = "Просунутий Python-програмування", ShortTitle = "Python Pro", ProviderTitle = "Школа програмування", ProviderTitleEn = "Programming School", WorkshopDraftStatus = WorkshopDraftStatus.PendingModeration, CreatedAt = baseDate.AddDays(5),ModifiedAt = baseDate.AddDays(5).AddHours(2)},
            new() {Title = "Історія", ShortTitle = "Pro", ProviderTitle = "Школа", ProviderTitleEn = "School", WorkshopDraftStatus = WorkshopDraftStatus.PendingModeration, CreatedAt = baseDate.AddDays(6),ModifiedAt = baseDate.AddDays(6).AddHours(2)},
            new() {Title = "Сучасна література", ShortTitle = "Python Pro", ProviderTitle = "Гурток творчого письма", ProviderTitleEn = "Creative Writing Club", WorkshopDraftStatus = WorkshopDraftStatus.EditedByModerator, CreatedAt = baseDate.AddDays(7),ModifiedAt = baseDate.AddDays(7).AddHours(2)}
        };
    }

    private class WorkshopDraftSeedConfig
    {
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public string ProviderTitle { get; set; }
        public string ProviderTitleEn { get; set; }
        public WorkshopDraftStatus WorkshopDraftStatus { get; set; }
        [DataType(DataType.DateTime)]
        public DateTimeOffset CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
