using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class LanguageServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private LanguageService service;
    private IEntityRepository<long, Language> repository;
    private Mock<ILogger<LanguageService>> logger;
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var builder = new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
            databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);

        repository = new EntityRepository<long, Language>(context);

        logger = new Mock<ILogger<LanguageService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();

        service = new LanguageService(repository, logger.Object, mapper);

        SeedDatabase();
    }

    [Test]
    public async Task GetAll_ReturnsAllLanguages_WhenLanguagesExist()
    {
        // Arrange
        List<Language> expected;
        using var ctx = new TestOutOfSchoolDbContext(options);
        {
            expected = ctx.Languages.ToList();
        }

        // Act
        var result = await service.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.First().Id, Is.EqualTo(expected.First().Id));
        Assert.That(result.Count(), Is.EqualTo(expected.Count));
        Assert.IsInstanceOf<IEnumerable<LanguageDto>>(result);
    }

    private void SeedDatabase()
    {
        using var ctx = new TestOutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            ctx.SaveChanges();
        }
    }
}
