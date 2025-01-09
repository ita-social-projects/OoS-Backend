using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using System;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CompetitiveEventAccountingTypeServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private IEntityRepositorySoftDeleted<int, CompetitiveEventAccountingType> repository;

    private Mock<ILogger<CompetitiveEventAccountingType>> logger;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private IMapper mapper;

    private CompetitiveEventAccountingTypeService service;

    [SetUp]
    public void SetUp()
    {
        var builder = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
        options = builder.Options;

        context = new OutOfSchoolDbContext(options);

        repository = new EntityRepositorySoftDeleted<int, CompetitiveEventAccountingType>(context);

        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        logger = new Mock<ILogger<CompetitiveEventAccountingType>>();

        service = new CompetitiveEventAccountingTypeService(
            repository,
            logger.Object,
            localizer.Object,
            mapper);

        SeedDataBase();
    }

    [Test]
    public async Task GetAll_WhenCalledWithLocalization_ReturnsLOcalizedResults()
    {
        // Arrange
        var expectedTitles = new[] { "Тип 10", "Тип 11" };

        // Act
        var result = await service.GetAll(LocalizationType.Ua).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(expectedTitles.Length, result.Count());

        var resultTitles = result.Select(x => x.Title).ToArray();
        CollectionAssert.AreEqual(expectedTitles, resultTitles);
    }
    [Test]
    public async Task GetAll_WhenCalledWithEnLocalization_ReturnsLocalizedResults()
    {
        // Arrange
        var expectedTitles = new[] { "AccountingType 10 En", "AccountingType 11 En" };

        // Act
        var result = await service.GetAll(LocalizationType.En).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(expectedTitles.Length, result.Count());

        var resultTitles = result.Select(x => x.Title).ToArray();
        CollectionAssert.AreEqual(expectedTitles, resultTitles);
    }

    [Test]
    public async Task GetAll_WhenRepositoryIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        context.CompetitiveEventAccountingTypes.RemoveRange(context.CompetitiveEventAccountingTypes);
        context.SaveChanges();

        // Act
        var result = await service.GetAll(LocalizationType.Ua).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TearDown]
    public void TearDown()
    {
        context.Database.EnsureDeleted();
    }
    private void SeedDataBase()
    {

        context.CompetitiveEventAccountingTypes.AddRange(new[]
        {
            new CompetitiveEventAccountingType { Id = 10, Title = "Тип 10", TitleEn = "AccountingType 10 En" },
            new CompetitiveEventAccountingType { Id = 11, Title = "Тип 11", TitleEn = "AccountingType 11 En" }
        });

        context.SaveChanges();
    }
}
