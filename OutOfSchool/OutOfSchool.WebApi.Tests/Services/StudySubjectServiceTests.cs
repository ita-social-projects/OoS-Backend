using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class StudySubjectServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private StudySubjectService service;
    private IEntityRepositorySoftDeleted<Guid, StudySubject> studySubjectRepository;
    private IEntityRepository<long, Language> languageRepository;
    private Mock<ILogger<StudySubjectService>> logger;
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var builder = new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
            databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new OutOfSchoolDbContext(options);

        studySubjectRepository = new EntityRepositorySoftDeleted<Guid, StudySubject>(context);
        languageRepository = new EntityRepository<long, Language>(context);

        logger = new Mock<ILogger<StudySubjectService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();

        service = new StudySubjectService(studySubjectRepository, languageRepository, logger.Object, mapper);

        SeedDatabase();
    }

    [Test]
    public async Task GetByFilter_ReturnsAListOfStudySubjects_WhenSearchStringIsEmpty()
    {
        // Arrange
        var expected = StudySubjects();

        // Act
        var result = await service.GetByFilter(null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.First().Id, Is.EqualTo(expected.First().Id));
        Assert.That(result.Count(), Is.EqualTo(expected.Count));
        Assert.IsInstanceOf<IEnumerable<StudySubjectDto>>(result);
    }

    [Test]
    public async Task GetByFilter_ReturnsAListOfFilteredStudySubjects_WhenSearchStringIsSpecified()
    {
        // Arrange
        var expected = StudySubjects().FirstOrDefault(x => x.NameInInstructionLanguage == "test");
        var filter = new SearchStringFilter() { SearchString = "test" };

        // Act
        var result = await service.GetByFilter(filter);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.First().Id, Is.EqualTo(expected.Id));
        Assert.IsInstanceOf<IEnumerable<StudySubjectDto>>(result);
    }

    [Test]
    public async Task GetByFilter_ReturnsEmptyList_WhenSearchStringDoesNotMatch()
    {
        // Arrange
        var filter = new SearchStringFilter() { SearchString = "nonexistent" };

        // Act
        var result = await service.GetByFilter(filter);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetById_ThrowsKeyNotFoundException_WhenStudySubjectDoesNotExist()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await service.GetById(id));

        // Assert
        Assert.That(ex.Message, Does.Contain("There are no recors in StudySubjects table with such id"));
    }

    [Test]
    public async Task GetById_ReturnsStudySubject_WhenItExists()
    {
        // Arrange
        var id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16");
        var expected = StudySubjects().FirstOrDefault(x => x.Id == id);

        // Act
        var result = await service.GetById(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(expected.Id));
        Assert.IsInstanceOf<StudySubjectDto>(result);
    }

    [Test]
    public async Task Create_ThrowsArgumentException_WhenDtoIsNull()
    {
        // Arrange
        StudySubjectCreateUpdateDto dto = null;

        // Act
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.Create(dto));

        // Assert
        Assert.That(ex.Message, Does.Contain("Dto is null"));
    }

    [Test]
    public async Task Create_CreatesStudySubject_WhenDtoIsValid()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            IsPrimaryLanguageUkrainian = true,
            LanguageIds = new List<long> { 2 },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я",
            PrimaryLanguageId = 2,
            WorkshopId = Guid.NewGuid(),
        };

        // Act
        var result = await service.Create(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(dto.Id));
        Assert.IsInstanceOf<StudySubjectCreateUpdateDto>(result);
    }

    [Test]
    public async Task Create_ThrowsArgumentException_WhenIdsAreInvalid()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            IsPrimaryLanguageUkrainian = true,
            LanguageIds = new List<long> { 99 },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я",
            PrimaryLanguageId = 77,
            WorkshopId = Guid.NewGuid(),
        };

        // Act
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.Create(dto));

        // Assert
        Assert.That(ex.Message, Does.Contain("Dto contains non-existing language ids"));
    }

    [Test]
    public async Task Update_ThrowsArgumentException_WhenDtoIsNull()
    {
        // Arrange
        StudySubjectCreateUpdateDto dto = null;

        // Act
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.Update(dto));

        // Assert
        Assert.That(ex.Message, Does.Contain("Dto is null"));
    }

    [Test]
    public async Task Update_ThrowsKeyNotFoundException_WhenStudySubjectWithIdDoesNotExist()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = Guid.Empty,
            IsPrimaryLanguageUkrainian = true,
            LanguageIds = new List<long> { 2 },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я",
            PrimaryLanguageId = 2,
            WorkshopId = Guid.NewGuid(),
        };

        // Act
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await service.Update(dto));

        // Assert
        Assert.That(ex.Message, Does.Contain("There are no recors in StudySubjects table with such id"));
    }

    [Test]
    public async Task Update_ReturnsStudySubject_WhenEntityWasUpdated()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16"),
            IsPrimaryLanguageUkrainian = true,
            LanguageIds = new List<long> { 2 },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я",
            PrimaryLanguageId = 2,
            WorkshopId = Guid.NewGuid(),
        };

        // Act
        var result = await service.Update(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(dto.Id));
        Assert.IsInstanceOf<StudySubjectCreateUpdateDto>(result);
    }

    [Test]
    public async Task Delete_ThrowsKeyNotFoundException_WhenStudySubjectWithIdDoesNotExist()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await service.Delete(id));

        // Assert
        Assert.That(ex.Message, Does.Contain($"StudySubject with Id = {id} does not exist or it was deleted."));
    }

    [Test]
    public async Task Delete_DeletesStudySubject_WhenEntityExists()
    {
        // Arrange
        var id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16");
        StudySubject result;

        // Act
        await service.Delete(id);
        using var ctx = new OutOfSchoolDbContext(options);
        {       
            result = await ctx.StudySubjects.FirstOrDefaultAsync(x => x.Id == id);
        }

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDeleted, Is.True);
    }

    private void SeedDatabase()
    {
        using var ctx = new OutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            ctx.StudySubjects.AddRange(StudySubjects());

            ctx.SaveChanges();
        }
    }

    private List<StudySubject> StudySubjects()
    {
        return new List<StudySubject>()
        {
            new StudySubject()
            {
                Id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16"),
                NameInInstructionLanguage = "тест",
                NameInUkrainian = "тест",
                PrimaryLanguageId = 2,
                IsPrimaryLanguageUkrainian = true,
                WorkshopId = new Guid("ce9d514c-8017-44f4-a1c0-2d758c64775c")
            },
            new StudySubject()
            {
                Id = new Guid("4ca6f3af-5d02-4c16-b4b2-e202c71470f4"),
                NameInInstructionLanguage = "test",
                NameInUkrainian = "тест",
                PrimaryLanguageId = 1,
                IsPrimaryLanguageUkrainian = false,
                WorkshopId = new Guid("ce9d514c-8017-44f4-a1c0-2d758c64775c")
            }
        };
    }
}
