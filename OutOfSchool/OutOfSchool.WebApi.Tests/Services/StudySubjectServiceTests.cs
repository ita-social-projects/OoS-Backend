using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class StudySubjectServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private StudySubjectService service;
    private IEntityRepositorySoftDeleted<Guid, StudySubject> studySubjectRepository;
    private IEntityRepository<long, Language> languageRepository;
    private Mock<ICurrentUserService> currentUserService;
    private Mock<ILogger<StudySubjectService>> logger;
    private IMapper mapper;
    private Guid providerId;
    private Guid studySubjectId;
    private Mock<IWorkshopRepository> workshopRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<Guid, StudySubject>> studySubjectRepositoryMock;
    private Mock<IEntityRepository<long, Language>> languageRepositoryMock;
    private Mock<IMapper> mapperMock;

    [SetUp]
    public void SetUp()
    {
        var builder = new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
            databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);

        studySubjectRepository = new EntityRepositorySoftDeleted<Guid, StudySubject>(context);
        languageRepository = new EntityRepository<long, Language>(context);
        providerId = Guid.NewGuid();

        currentUserService = new Mock<ICurrentUserService>();
        logger = new Mock<ILogger<StudySubjectService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        workshopRepositoryMock = new Mock<IWorkshopRepository>();

        service = new StudySubjectService(
            studySubjectRepository, 
            workshopRepositoryMock.Object,
            languageRepository,
            currentUserService.Object,
            logger.Object, 
            mapper
            );

        SeedDatabase();
    }

    #region GetByFilter

    [Test]
    public async Task GetByFilter_ReturnsAListOfStudySubjects_WhenSearchStringIsEmpty()
    {
        // Arrange
        var expected = StudySubjects();

        // Act
        var result = await service.GetByFilter(providerId, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Entities.First().Id, Is.EqualTo(expected.First().Id));
        Assert.That(result.TotalAmount, Is.EqualTo(expected.Count));
        Assert.IsInstanceOf<SearchResult<StudySubjectDto>>(result);
    }
    
    [Test]
    public async Task GetByFilter_ReturnsAListOfFilteredStudySubjects_WhenSearchStringIsSpecified()
    {
        // Arrange
        var expected = StudySubjects().FirstOrDefault(x => x.NameInInstructionLanguage == "test");
        var filter = new StudySubjectFilter() { SearchString = "test" };

        // Act
        var result = await service.GetByFilter(providerId, filter);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Entities.First().Id, Is.EqualTo(expected.Id));
        Assert.IsInstanceOf<SearchResult<StudySubjectDto>>(result);
    }
    
    [Test]
    public async Task GetByFilter_ReturnsEmptyList_WhenSearchStringDoesNotMatch()
    {
        // Arrange
        var filter = new StudySubjectFilter() { 
            SearchString = "nonexistent" , 
            StartDate = DateTime.Today, 
            EndDate = DateTime.Today};

        // Act
        var result = await service.GetByFilter(providerId, filter);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Entities, Is.Empty);
    }

    #endregion

    #region GetById

    [Test]
    public async Task GetById_ReturnsNull_WhenStudySubjectDoesNotExist()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var result = await service.GetById(id, providerId);

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetById_ReturnsStudySubject_WhenItExists()
    {
        // Arrange
        var id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16");
        var expected = StudySubjects().FirstOrDefault(x => x.Id == id);

        // Act
        var result = await service.GetById(id, providerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(expected.Id));
        Assert.IsInstanceOf<StudySubjectDto>(result);
    }

    #endregion

    #region Create

    [Test]
    public async Task Create_ReturnsNull_WhenDtoIsNull()
    {
        // Arrange
        StudySubjectCreateUpdateDto dto = null;

        // Act
        var result = await service.Create(dto, providerId);

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task Create_CreatesStudySubject_WhenDtoIsValid()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            IsLanguageUkrainian = true,
            Language = new LanguageDto()
            {
                Id = 2,
                Code = "Ua",
                Name = "Українська"
            },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я",
        };

        // Act
        var result = await service.Create(dto, providerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(dto.Id));
        Assert.IsInstanceOf<StudySubjectDto>(result);
    }

    [Test]
    public async Task Create_AddsUkrainianLanguageToInvalidEntity_WhenUkrainianMarkedAsPrimaryLanguage()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = Guid.NewGuid(),
            IsLanguageUkrainian = true,
            Language = new LanguageDto()
            {
                Id = 1,
                Code = "Ua",
                Name = "Українська"
            },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я",
        };

        // Act
        var result = await service.Create(dto, providerId).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(dto.Id));
        Assert.That(result.LanguageId == 2);
        Assert.IsInstanceOf<StudySubjectDto>(result);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_ReturnsResultFailed_WhenDtoIsNull()
    {
        // Arrange
        StudySubjectCreateUpdateDto dto = null;

        // Act
        var result = await service.Update(dto, providerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.IsInstanceOf<Result<StudySubjectDto>>(result);
    }
    
    [Test]
    public async Task Update_ReturnsResultFailed_WhenStudySubjectWithIdDoesNotExist()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = Guid.Empty,
            IsLanguageUkrainian = true,
            Language = new LanguageDto()
            {
                Id = 2,
                Code = "Ua",
                Name = "Українська"
            },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я"
        };

        // Act
        var result = await service.Update(dto, providerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("404"));
        Assert.IsInstanceOf<Result<StudySubjectDto>>(result);
    }

    [Test]
    public async Task Update_ReturnsResultFailed_WhenDbUpdateConcurrencyExceptionOccurs()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16"),
            IsLanguageUkrainian = true,
            Language = new LanguageDto()
            {
                Id = 2,
                Code = "Ua",
                Name = "Українська"
            },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я",
        };

        var mockRepository = SetUpMockRepositoryForGetById(dto.Id);
        mockRepository
            .Setup(repo => repo.Update(It.IsAny<StudySubject>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        service = new StudySubjectService(
            mockRepository.Object,
            workshopRepositoryMock.Object,
            languageRepository,
            currentUserService.Object,
            logger.Object,
            mapper);

        // Act
        var result = await service.Update(dto, providerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.OperationResult.Errors, Is.Not.Null);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description,
                    Is.EqualTo("Updating failed. StudySubject to update was not found"));
        Assert.IsInstanceOf<Result<StudySubjectDto>>(result);
    }

    [Test]
    public async Task Update_ReturnsResultSuccess_WhenEntityWasUpdated()
    {
        // Arrange
        var dto = new StudySubjectCreateUpdateDto()
        {
            Id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16"),
            IsLanguageUkrainian = true,
            Language = new LanguageDto()
            {
                Id = 2,
                Code = "Ua",
                Name = "Українська"
            },
            NameInInstructionLanguage = "ім'я",
            NameInUkrainian = "ім'я"
        };

        // Act
        var result = await service.Update(dto, providerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.Id, Is.EqualTo(dto.Id));
        Assert.IsInstanceOf<Result<StudySubjectDto>>(result);
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_ReturnsResutlFailed_WhenStudySubjectWithIdDoesNotExist()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var result = await service.Delete(id, providerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("404"));
        Assert.IsInstanceOf<Result<StudySubjectDto>>(result);
    }
    
    [Test]
    public async Task Delete_DeletesStudySubject_WhenEntityExists()
    {
        // Arrange
        var id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16");
        StudySubject result;

        // Act
        await service.Delete(id, providerId);
        using var ctx = new TestOutOfSchoolDbContext(options);
        {       
            result = await ctx.StudySubjects.FirstOrDefaultAsync(x => x.Id == id);
        }

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDeleted, Is.True);
    }

    [Test]
    public async Task Delete_ReturnsResultFailed_WhenDbUpdateConcurrencyExceptionOccurs()
    {
        // Arrange
        var id = new Guid("eb49a87c-7042-45e9-a76b-79ebd98b6b16");
        
        var mockRepository = SetUpMockRepositoryForGetById(id);
        mockRepository
            .Setup(repo => repo.Delete(It.IsAny<StudySubject>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        service = new StudySubjectService(
            mockRepository.Object,
            workshopRepositoryMock.Object,
            languageRepository,
            currentUserService.Object,
            logger.Object,
            mapper);

        // Act
        var result = await service.Delete(id, providerId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.OperationResult.Errors, Is.Not.Null);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Description,
                    Is.EqualTo($"Deleting StudySubject with Id = {id} failed"));
        Assert.IsInstanceOf<Result<StudySubjectDto>>(result);
    }

    #endregion

    #region UpdateWorkshopsForStudySubject

    [Test]
    public async Task UpdateWorkshopsForStudySubject_ReturnsNotFound_WhenStudySubjectNotFound()
    {
        // Arrange
        SetupWithMocks();

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync((StudySubject)null);

        var workshops = new List<WorkshopAttachmentStatusDto>();

        // Act
        var result = await service.UpdateWorkshopsForStudySubject(studySubjectId, providerId, workshops);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo("404"));
    }

    [Test]
    public async Task UpdateWorkshopsForStudySubject_ReturnsNotFound_WhenProviderHasNoWorkshops()
    {
        // Arrange
        SetupWithMocks();

        var studySubject = new StudySubject { Id = studySubjectId, ProviderId = providerId, Workshops = new List<Workshop>() };
        var workshopsWithStatus = new List<WorkshopAttachmentStatusDto>
        {
            new() { Id = Guid.NewGuid(), IsAttached = true, Title = "Some Workshop" }
        };

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync(studySubject);

        workshopRepositoryMock
            .Setup(repo => repo.GetByFilter(
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Workshop>, IQueryable<Workshop>>>()))
            .ReturnsAsync(new List<Workshop>());

        // Act
        var result = await service.UpdateWorkshopsForStudySubject(studySubjectId, providerId, workshopsWithStatus);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo("404"));
    }

    [Test]
    public async Task UpdateWorkshopsForStudySubject_DetachesWorkshops_WhenIsAttachedIsTrue()
    {
        // Arrange
        SetupWithMocks();

        var workshopToDetach = new Workshop { Id = Guid.NewGuid(), ProviderId = providerId };
        var studySubject = new StudySubject
        {
            Id = studySubjectId,
            ProviderId = providerId,
            Workshops = new List<Workshop> { workshopToDetach }
        };

        var workshopsWithStatus = new List<WorkshopAttachmentStatusDto>
        {
            new() { Id = workshopToDetach.Id, IsAttached = true, Title = "Detach Me" }
        };

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync(studySubject);

        workshopRepositoryMock
            .Setup(repo => repo.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), "", null))
            .ReturnsAsync(new List<Workshop> { workshopToDetach });

        // Act
        var result = await service.UpdateWorkshopsForStudySubject(studySubjectId, providerId, workshopsWithStatus);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(studySubject.Workshops, Is.Empty);
    }

    [Test]
    public async Task UpdateWorkshopsForStudySubject_AttachesWorkshops_WhenIsAttachedIsFalse()
    {
        // Arrange
        SetupWithMocks();

        var workshopToAttach = new Workshop { Id = Guid.NewGuid(), ProviderId = providerId };
        var studySubject = new StudySubject
        {
            Id = studySubjectId,
            ProviderId = providerId,
            Workshops = new List<Workshop>()
        };

        var workshopsWithStatus = new List<WorkshopAttachmentStatusDto>
        {
            new() { Id = workshopToAttach.Id, IsAttached = false, Title = "Attach Me" }
        };

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync(studySubject);

        workshopRepositoryMock
            .Setup(repo => repo.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), "", null))
            .ReturnsAsync(new List<Workshop> { workshopToAttach });

        // Act
        var result = await service.UpdateWorkshopsForStudySubject(studySubjectId, providerId, workshopsWithStatus);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(studySubject.Workshops, Has.Count.EqualTo(1));
        Assert.That(studySubject.Workshops.First().Id, Is.EqualTo(workshopToAttach.Id));
    }

    [Test]
    public async Task UpdateWorkshopsForStudySubject_ReturnsFailed_WhenDbUpdateConcurrencyExceptionOccurs()
    {
        // Arrange
        SetupWithMocks();

        var workshop = new Workshop { Id = Guid.NewGuid(), ProviderId = providerId };

        var studySubject = new StudySubject
        {
            Id = studySubjectId,
            ProviderId = providerId,
            Workshops = new List<Workshop> { workshop }
        };

        var workshopsWithStatus = new List<WorkshopAttachmentStatusDto>
        {
            new() { Id = workshop.Id, IsAttached = false, Title = "Attach Me" }
        };

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync(studySubject);

        workshopRepositoryMock
            .Setup(repo => repo.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), "", null))
            .ReturnsAsync(new List<Workshop> { workshop });

        studySubjectRepositoryMock
            .Setup(repo => repo.Update(studySubject))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var result = await service.UpdateWorkshopsForStudySubject(studySubjectId, providerId, workshopsWithStatus);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo("400"));
    }

    [Test]
    public async Task UpdateWorkshopsForStudySubject_ReturnsNotFound_WhenStudySubjectNotExists()
    {
        // Arrange
        SetupWithMocks();

        currentUserService
            .Setup(x => x.UserHasRights(It.Is<ProviderRights[]>(rights => rights.Length == 1 && rights[0].providerId == providerId)))
            .Returns(Task.CompletedTask);

        studySubjectRepositoryMock
            .Setup(s => s.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync((StudySubject)null);

        // Act
        var result = await service.UpdateWorkshopsForStudySubject(
            studySubjectId, 
            providerId, 
            new List<WorkshopAttachmentStatusDto>());

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo("404"));
    }

    [Test]
    public void UpdateWorkshopsForStudySubject_ThrowsUnauthorized_WhenProviderHasNoRights()
    {
        // Arrange
        currentUserService
            .Setup(x => x.UserHasRights(It.IsAny<IUserRights[]>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.UpdateWorkshopsForStudySubject(
                studySubjectId, 
                providerId, 
                new List<WorkshopAttachmentStatusDto>())
        );
    }

    #endregion

    #region DetachAllWorkshops

    [Test]
    public async Task DetachAllWorkshops_ReturnsNotFound_WhenStudySubjectNotFound()
    {
        // Arrange
        SetupWithMocks();

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync((StudySubject)null);

        // Act
        var result = await service.DetachAllWorkshops(studySubjectId, providerId);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo("404"));
    }

    [Test]
    public async Task DetachAllWorkshops_ReturnsSuccess_WhenNoWorkshopsAttached()
    {
        // Arrange
        SetupWithMocks();

        var studySubject = new StudySubject { Id = studySubjectId, ProviderId = providerId, Workshops = new List<Workshop>() };

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync(studySubject);

        // Act
        var result = await service.DetachAllWorkshops(studySubjectId, providerId);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public async Task DetachAllWorkshops_ReturnsNotFound_WhenProviderHasNoWorkshops()
    {
        // Arrange
        SetupWithMocks();

        var studySubject = new StudySubject
        {
            Id = studySubjectId,
            ProviderId = providerId,
            Workshops = new List<Workshop>
            {
                new Workshop { Id = Guid.NewGuid(), ProviderId = Guid.NewGuid() }
            }
        };

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync(studySubject);

        workshopRepositoryMock
            .Setup(repo => repo.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), "", null))
            .ReturnsAsync(new List<Workshop>());

        // Act
        var result = await service.DetachAllWorkshops(studySubjectId, providerId);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo("404"));
    }

    [Test]
    public async Task DetachAllWorkshops_RemovesOnlyProviderWorkshops()
    {
        // Arrange
        SetupWithMocks();

        var providerWorkshop1 = new Workshop { Id = Guid.NewGuid(), ProviderId = providerId };
        var providerWorkshop2 = new Workshop { Id = Guid.NewGuid(), ProviderId = providerId };
        var foreignWorkshop = new Workshop { Id = Guid.NewGuid(), ProviderId = Guid.NewGuid() };

        var studySubject = new StudySubject
        {
            Id = studySubjectId,
            ProviderId = providerId,
            Workshops = new List<Workshop> { providerWorkshop1, providerWorkshop2, foreignWorkshop }
        };

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync(studySubject);

        workshopRepositoryMock
            .Setup(repo => repo.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), "", null))
            .ReturnsAsync(new List<Workshop> { providerWorkshop1, providerWorkshop2 });

        // Act
        var result = await service.DetachAllWorkshops(studySubjectId, providerId);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(studySubject.Workshops, Has.Count.EqualTo(1));
        Assert.That(studySubject.Workshops.First().Id, Is.EqualTo(foreignWorkshop.Id));
    }

    [Test]
    public async Task DetachAllWorkshops_ReturnsFailed_WhenDbUpdateConcurrencyExceptionOccurs()
    {
        // Arrange
        SetupWithMocks();

        var providerWorkshop = new Workshop { Id = Guid.NewGuid(), ProviderId = providerId };

        var studySubject = new StudySubject
        {
            Id = studySubjectId,
            ProviderId = providerId,
            Workshops = new List<Workshop> { providerWorkshop }
        };

        studySubjectRepositoryMock
            .Setup(repo => repo.GetByIdWithDetails(studySubjectId, "Workshops", null))
            .ReturnsAsync(studySubject);

        workshopRepositoryMock
            .Setup(repo => repo.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), "", null))
            .ReturnsAsync(new List<Workshop> { providerWorkshop });

        studySubjectRepositoryMock
            .Setup(repo => repo.Update(studySubject))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var result = await service.DetachAllWorkshops(studySubjectId, providerId);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.First().Code, Is.EqualTo("400"));
    }

    #endregion

    private void SetupWithMocks()
    {
        studySubjectRepositoryMock = new Mock<IEntityRepositorySoftDeleted<Guid, StudySubject>>();
        languageRepositoryMock = new Mock<IEntityRepository<long, Language>>();
        workshopRepositoryMock = new Mock<IWorkshopRepository>();
        currentUserService = new Mock<ICurrentUserService>();
        logger = new Mock<ILogger<StudySubjectService>>();
        mapperMock = new Mock<IMapper>();

        providerId = Guid.NewGuid();
        studySubjectId = Guid.NewGuid();

        mapperMock.Setup(m => m.Map<StudySubjectDto>(It.IsAny<StudySubject>()))
        .Returns((StudySubject s) => new StudySubjectDto
        {
            Id = s.Id,
            NameInUkrainian = s.NameInUkrainian,
            NameInInstructionLanguage = s.NameInInstructionLanguage,
            IsLanguageUkrainian = s.IsLanguageUkrainian,
            LanguageId = s.LanguageId,
            ProviderId = s.ProviderId,
            Workshops = s.Workshops?
                .Select(w => new ShortEntityDto { Id = w.Id, Title = w.Title })
                .ToList() ?? new List<ShortEntityDto>(),
            ActiveFrom = s.ActiveFrom,
            ActiveTo = s.ActiveTo
        });

        service = new StudySubjectService(
            studySubjectRepositoryMock.Object,
            workshopRepositoryMock.Object,
            languageRepositoryMock.Object,
            currentUserService.Object,
            logger.Object,
            mapper
        );
    }


    private void SeedDatabase()
    {
        using var ctx = new TestOutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            ctx.StudySubjects.AddRange(StudySubjects());

            ctx.SaveChanges();
        }
    }

    private Mock<IEntityRepositorySoftDeleted<Guid, StudySubject>> SetUpMockRepositoryForGetById(Guid id)
    {
        var studySubject = new StudySubject()
        {
            Id = id,
            LanguageId = 2,
            NameInInstructionLanguage = "тест",
            NameInUkrainian = "тест",
            IsLanguageUkrainian = true,
        };

        var mockRepository = new Mock<IEntityRepositorySoftDeleted<Guid, StudySubject>>();
        mockRepository
            .Setup(repo => repo.GetById(id))
            .ReturnsAsync(studySubject);

        return mockRepository;
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
                LanguageId = 2,
                IsLanguageUkrainian = true
            },
            new StudySubject()
            {
                Id = new Guid("4ca6f3af-5d02-4c16-b4b2-e202c71470f4"),
                NameInInstructionLanguage = "test",
                NameInUkrainian = "тест",
                LanguageId = 1,
                IsLanguageUkrainian = false
            }
        };
    }
    
}
