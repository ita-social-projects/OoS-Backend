using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.ExternalFileStore.Models;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.SubordinationStructure;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class WorkshopDraftServiceTests
{
    private IWorkshopDraftService service;  
    private Mock<IWorkshopDraftRepository> workshopDraftRepoMoq;
    private Mock<IInstitutionHierarchyService> institutionHierarchyServiceMock;
    private Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>> workshopDraftImagesServiceMock;
    private Mock<IEntityCoverImageInteractionService<TeacherDraft>> teacherDraftImagesServiceMock;
    private Mock<ILanguageService> languageServiceMoq;
    private Mock<IProviderService> providerServiceMoq;
    private Mock<ICurrentUserService> currentUserServiceMoq;
    private Mock<IEntityRepository<long, Tag>> tagRepositoryMoq;
    private Mock<IWorkshopServicesCombinerV2> workshopServiceCombinerV2Moq;
    private Mock<IInstitutionHierarchyRepository> institutionHierarchyRepositoryMoq;
    private Mock<ICodeficatorRepository> codeficatorRepositoryMoq;
    private Mock<IChangesLogService> changesLogServiceMock;
    private Mock<IOptions<InstitutionOptions>> institutionOptionsMock;

    private string userId;

    [SetUp]
    public void SetUp()
    {
        workshopDraftRepoMoq = new Mock<IWorkshopDraftRepository>();
        workshopDraftImagesServiceMock = new Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>>();
        teacherDraftImagesServiceMock = new Mock<IEntityCoverImageInteractionService<TeacherDraft>>();
        currentUserServiceMoq = new Mock<ICurrentUserService>();
        institutionHierarchyServiceMock = new  Mock<IInstitutionHierarchyService>();
        providerServiceMoq = new Mock<IProviderService>();
        tagRepositoryMoq = new Mock<IEntityRepository<long, Tag>>();
        workshopServiceCombinerV2Moq = new Mock<IWorkshopServicesCombinerV2>();
        institutionHierarchyRepositoryMoq = new Mock<IInstitutionHierarchyRepository>();
        codeficatorRepositoryMoq = new Mock<ICodeficatorRepository>();
        languageServiceMoq = new Mock<ILanguageService>();
        changesLogServiceMock = new Mock<IChangesLogService>();


        var options = new Mock<IOptions<UploadConcurrencySettings>>();
        var settings = new UploadConcurrencySettings();
        options.Setup(o => o.Value).Returns(settings);

        var logger = new Mock<ILogger<WorkshopDraftService>>();
        var workshopDraftImagesService = new Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>>();   
        var teacherDraftImagesService = new Mock<IEntityCoverImageInteractionService<TeacherDraft>>();       
        var regionAdminService = new Mock<IRegionAdminService>();
        var ministryAdminService = new Mock<IMinistryAdminService>();
        var codeficatorService = new Mock<ICodeficatorService>();
        var searchStringService = new Mock<ISearchStringService>();              
        institutionOptionsMock = new Mock<IOptions<InstitutionOptions>>();
      
        userId = "someUserId";
        service = new WorkshopDraftService(
                   logger.Object,
                   languageServiceMoq.Object,
                   workshopDraftRepoMoq.Object,
                   workshopDraftImagesService.Object,
                   providerServiceMoq.Object,
                   currentUserServiceMoq.Object,
                   teacherDraftImagesService.Object,
                   tagRepositoryMoq.Object,
                   options.Object,
                   workshopServiceCombinerV2Moq.Object,
                   regionAdminService.Object,
                   ministryAdminService.Object,
                   codeficatorService.Object,
                   searchStringService.Object,
                   institutionHierarchyRepositoryMoq.Object,
                   codeficatorRepositoryMoq.Object,
                   changesLogServiceMock.Object,
                   institutionOptionsMock.Object);
        
        institutionOptionsMock.Setup(x => x.Value)
            .Returns(new InstitutionOptions { MinistryOfSportTitle = "Мінспорт" });
        SetupInstitutionHierarchy();
    }

    #region Create
    [Test]
    public void Create_WithNullDto_ShouldThrowArgumentNullException()
    {
        // Arrange
        WorkshopV2Dto workshopV2Dto = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await service.Create(workshopV2Dto));
    }

    [Test]
    public async Task Create_WithValidDto_ShouldReturnCreatedObject()
    {
        // Arrange
        var institutionHierarchyId = Guid.NewGuid();
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers().WithLanguage();
        workshop.InstitutionHierarchyId = institutionHierarchyId;
        
        var workshopV2Dto = workshop.ToV2Dto();
        workshopV2Dto.InstitutionHierarchyId = institutionHierarchyId;
        
        var workshopDraft = workshopV2Dto.ToDraft();
        workshopDraft.WorkshopDraftContent = new WorkshopDraftContent
        {
            InstitutionHierarchyId = institutionHierarchyId
        };
        var workshopResponse = workshopDraft.ToResponseDto();

        institutionHierarchyRepositoryMoq.Setup(x => x.GetById(institutionHierarchyId))
            .ReturnsAsync(new InstitutionHierarchy
            {
                Id = institutionHierarchyId,
                Institution = new Institution { Title = "Мінспорт" },
            });
        languageServiceMoq.Setup(x => x.GetById(workshop.LanguageOfEducationId))
            .ReturnsAsync(new LanguageDto { Id = workshop.LanguageOfEducationId, Name = workshop.LanguageOfEducation.Name });

        workshopDraftRepoMoq.Setup(x => x.RunInTransaction(It.IsAny<Func<Task<WorkshopDraft>>>()))
            .ReturnsAsync(workshopDraft);
        tagRepositoryMoq
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Tag, bool>>>(), 
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Tag>, IQueryable<Tag>>>()))
            .ReturnsAsync(Enumerable.Empty<Tag>()).Verifiable(Times.Once);
        codeficatorRepositoryMoq.Setup(x => x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsQueryable().BuildMock());

        // Act 
        var result = await service.Create(workshopV2Dto).ConfigureAwait(false);

        //Assert 
        currentUserServiceMoq.VerifyAll();
        workshopDraftRepoMoq.VerifyAll();
        tagRepositoryMoq.VerifyAll();

        result.Should().NotBeNull();
        result.WorkshopDraft.Should().BeEquivalentTo(workshopResponse);
    }
    [Test]
    public void Create_WithInvalidLanguageId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto();
        workshopV2Dto.LanguageOfEducationId = 123213213; // invalid ID

        languageServiceMoq.Setup(x => x.GetById(workshopV2Dto.LanguageOfEducationId))
            .ReturnsAsync((LanguageDto)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Create(workshopV2Dto));
        ex.Message.Should().Contain($"Language with ID = {workshopV2Dto.LanguageOfEducationId}");
    }
    
    [Test]
    public async Task Create_WhenInstitutionIsMinSport_ShouldSetChampionPathAndSectionType()
    {
        // Arrange
        var institutionHierarchyId = Guid.NewGuid();
        var languageId = 1L;
        var languageName = "Українська";

        var workshop = WorkshopGenerator.Generate()
            .WithProvider()
            .WithTeachers()
            .WithLanguage(languageId, languageName);

        workshop.InstitutionHierarchyId = institutionHierarchyId;

        var workshopV2Dto = workshop.ToV2Dto();
        workshopV2Dto.InstitutionHierarchyId = institutionHierarchyId;

        institutionHierarchyRepositoryMoq.Setup(x => x.GetById(institutionHierarchyId))
            .ReturnsAsync(new InstitutionHierarchy
            {
                Id = institutionHierarchyId,
                Institution = new Institution { Title = "Мінспорт" },
            });

        languageServiceMoq.Setup(x => x.GetById(languageId))
            .ReturnsAsync(new LanguageDto
            {
                Id = languageId,
                Name = languageName
            });

        providerServiceMoq
            .Setup(x => x.GetLicenseStatusAndOwnershipAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Tuple.Create(ProviderLicenseStatus.Approved, OwnershipType.State));

        tagRepositoryMoq
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Tag>, IQueryable<Tag>>>()))
            .ReturnsAsync(Enumerable.Empty<Tag>());

        codeficatorRepositoryMoq.Setup(x => x.Get(It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsQueryable().BuildMock());

        workshopDraftImagesServiceMock
            .Setup(x => x.AddCoverImageAsync(It.IsAny<WorkshopDraft>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result<string>.Success("cover-id"));

        teacherDraftImagesServiceMock
            .Setup(x => x.AddCoverImageAsync(It.IsAny<TeacherDraft>(), It.IsAny<IFormFile>()))
            .ReturnsAsync(Result<string>.Success("teacher-cover-id"));

        var expectedDraft = new WorkshopDraft
        {
            Id = Guid.NewGuid(),
            WorkshopDraftContent = new WorkshopDraftContent
            {
                InstitutionHierarchyId = institutionHierarchyId,
                IsChampionPath = true,
                WorkshopType = WorkshopType.Section
            },
            Teachers = new List<TeacherDraft>()
        };

        workshopDraftRepoMoq
            .Setup(x => x.RunInTransaction(It.IsAny<Func<Task<WorkshopDraft>>>()))
            .ReturnsAsync(expectedDraft);

        // Act
        var result = await service.Create(workshopV2Dto);

        // Assert
        result.Should().NotBeNull();
        result.WorkshopDraft.Should().NotBeNull();
        
        var details = result.WorkshopDraft.WorkshopDetails;

        details.IsChampionPath.Should().BeTrue("Institution is Мінспорт");
        details.WorkshopType.Should().Be(WorkshopType.Section, "Institution is Мінспорт");
        details.InstitutionHierarchyId.Should().Be(institutionHierarchyId);
    }
    
    [Test]
    public async Task Create_WhenInstitutionIsNotMinSport_ShouldNotSetIsChampionPathAndWorkshopType()
    {
        // Arrange
        var institutionHierarchyId = Guid.NewGuid();
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers().WithLanguage();
        workshop.InstitutionHierarchyId = institutionHierarchyId;

        var workshopV2Dto = workshop.ToV2Dto();
        workshopV2Dto.InstitutionHierarchyId = institutionHierarchyId;
        workshopV2Dto.WorkshopType = WorkshopType.Workshop;

        var workshopDraft = workshopV2Dto.ToDraft();
        workshopDraft.WorkshopDraftContent = new WorkshopDraftContent
        {
            InstitutionHierarchyId = institutionHierarchyId
        };

        var expectedResponse = workshopDraft.ToResponseDto();
        expectedResponse.WorkshopDetails.IsChampionPath = false;
        expectedResponse.WorkshopDetails.WorkshopType = WorkshopType.Workshop;

        institutionHierarchyRepositoryMoq.Setup(x => x.GetById(institutionHierarchyId))
            .ReturnsAsync(new InstitutionHierarchy
            {
                Id = institutionHierarchyId,
                Institution = new Institution { Title = "NotMinSport" },
            });

        languageServiceMoq.Setup(x => x.GetById(workshop.LanguageOfEducationId))
            .ReturnsAsync(new LanguageDto
            {
                Id = workshop.LanguageOfEducationId,
                Name = workshop.LanguageOfEducation.Name
            });

        workshopDraftRepoMoq.Setup(x => x.RunInTransaction(It.IsAny<Func<Task<WorkshopDraft>>>()))
            .ReturnsAsync(workshopDraft);

        tagRepositoryMoq
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Tag, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Tag>, IQueryable<Tag>>>()))
            .ReturnsAsync(Enumerable.Empty<Tag>());

        codeficatorRepositoryMoq.Setup(x => x.Get(It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Expression<Func<CATOTTG, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsQueryable().BuildMock());

        // Act
        var result = await service.Create(workshopV2Dto);

        // Assert
        result.Should().NotBeNull();
        result.WorkshopDraft.Should().BeEquivalentTo(expectedResponse);
    }
    #endregion

    #region Update
    [Test]
    public void Update_WithNullDto_ShouldThrowArgumentNullException()
    {
        // Arrange
        WorkshopDraftUpdateDto workshopDraftUpdateDto = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await service.Update(workshopDraftUpdateDto));
    }

    [Test]
    public async Task Update_WithValidDto_ShouldReturnUpdatedObject()
    {
        //Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers().WithLanguage();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();
        var workshopResponse = workshopDraft.ToResponseDto();
        
        workshopResponse.WorkshopDetails.IsChampionPath = true;
        workshopResponse.WorkshopDetails.WorkshopType = WorkshopType.Section;
        
        var workshopUpdateDto = new WorkshopDraftUpdateDto()
        {
            Id = Guid.NewGuid(),
            WorkshopV2Dto = workshopV2Dto
        };
        
        institutionHierarchyRepositoryMoq.Setup(x => x.GetById(workshop.InstitutionHierarchyId.Value))
            .ReturnsAsync(new InstitutionHierarchy
            {
                Id = workshop.InstitutionHierarchyId.Value,
                Institution = new Institution { Title = "Мінспорт" },
            });
        
        languageServiceMoq.Setup(x => x.GetById(workshopV2Dto.LanguageOfEducationId))
            .ReturnsAsync(new LanguageDto { Id = workshopV2Dto.LanguageOfEducationId, Name = workshopV2Dto.LanguageOfEducationName });
        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(workshopV2Dto);
        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq
            .Setup(x => x.RunInTransaction(
                It.IsAny<Func<Task<(WorkshopDraft,
                ImageChangingResult,
                MultipleImageChangingResult,
                List<TeacherCreateUpdateResultDto>)>>>()))
            .Returns((Func<Task<(WorkshopDraft,
                ImageChangingResult,
                MultipleImageChangingResult,
                List<TeacherCreateUpdateResultDto>)>> f) => f.Invoke())
            .Verifiable(Times.Once);
        codeficatorRepositoryMoq.Setup(x => x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsQueryable().BuildMock());

        //Act
        var result = await service.Update(workshopUpdateDto).ConfigureAwait(false);

        //Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();          

        result.Should().NotBeNull();
        result.WorkshopDraft.Should().BeEquivalentTo(workshopResponse);
    }

    [Test]
    public void Update_WithInvalidLanguageId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers().WithLanguage();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();

        var updateDto = new WorkshopDraftUpdateDto
        {
            Id = Guid.NewGuid(),
            WorkshopV2Dto = workshopV2Dto
        };

        workshopDraftRepoMoq.Setup(x => x.GetById(updateDto.Id)).ReturnsAsync(workshopDraft);

        workshopDraftRepoMoq
            .Setup(x => x.RunInTransaction(It.IsAny<Func<Task<(WorkshopDraft, ImageChangingResult, MultipleImageChangingResult, List<TeacherCreateUpdateResultDto>)>>>()))
            .Returns((Func<Task<(WorkshopDraft, ImageChangingResult, MultipleImageChangingResult, List<TeacherCreateUpdateResultDto>)>> f) => f());

        languageServiceMoq.Setup(x => x.GetById(workshopV2Dto.LanguageOfEducationId))
            .ReturnsAsync((LanguageDto)null);

        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(workshopV2Dto);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Update(updateDto));
        ex.Message.Should().Contain($"Language with ID = {workshopV2Dto.LanguageOfEducationId}");
    }
    
    [Test]
    public async Task Update_WhenInstitutionIsMinSport_ShouldSetChampionPathAndSectionType()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers().WithLanguage();
        var institutionHierarchyId = Guid.NewGuid();
        workshop.InstitutionHierarchyId = institutionHierarchyId;

        var workshopV2Dto = workshop.ToV2Dto();
        workshopV2Dto.Id = Guid.NewGuid(); // simulate update
        workshopV2Dto.InstitutionHierarchyId = institutionHierarchyId;

        var updateDto = new WorkshopDraftUpdateDto
        {
            Id = Guid.NewGuid(),
            WorkshopV2Dto = workshopV2Dto
        };

        var updatedDraft = new WorkshopDraft
        {
            Id = updateDto.Id,
            WorkshopDraftContent = new WorkshopDraftContent
            {
                InstitutionHierarchyId = institutionHierarchyId,
                IsChampionPath = true,
                WorkshopType = WorkshopType.Section
            },
            Teachers = new List<TeacherDraft>()
        };
        
        codeficatorRepositoryMoq.Setup(x => x.Get(It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsQueryable().BuildMock());
        
        institutionHierarchyRepositoryMoq.Setup(x => x.GetById(institutionHierarchyId))
            .ReturnsAsync(new InstitutionHierarchy
            {
                Id = institutionHierarchyId,
                Institution = new Institution { Title = "Мінспорт" }
            });

        languageServiceMoq.Setup(x => x.GetById(workshopV2Dto.LanguageOfEducationId))
            .ReturnsAsync(new LanguageDto { Id = workshopV2Dto.LanguageOfEducationId, Name = workshopV2Dto.LanguageOfEducationName });

        workshopDraftRepoMoq
            .Setup(x => x.GetById(updateDto.Id))
            .ReturnsAsync(updatedDraft);

        workshopDraftRepoMoq
            .Setup(x => x.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync(updatedDraft);

        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), true))
            .ReturnsAsync(workshopV2Dto);

        workshopDraftRepoMoq
            .Setup(x => x.RunInTransaction(It.IsAny<Func<Task<(WorkshopDraft, ImageChangingResult, MultipleImageChangingResult, List<TeacherCreateUpdateResultDto>)>>>()))
            .Returns<Func<Task<(WorkshopDraft, ImageChangingResult, MultipleImageChangingResult, List<TeacherCreateUpdateResultDto>)>>>(f => f());

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.WorkshopDraft.Should().NotBeNull();
        result.WorkshopDraft.WorkshopDetails.IsChampionPath.Should().BeTrue();
        result.WorkshopDraft.WorkshopDetails.WorkshopType.Should().Be(WorkshopType.Section);
    }

    [Test]
    public async Task Update_WhenInstitutionIsNotMinSport_ShouldNotSetIsChampionPathAndWorkshopType()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers().WithLanguage();
        var institutionHierarchyId = Guid.NewGuid();
        workshop.InstitutionHierarchyId = institutionHierarchyId;

        var workshopV2Dto = workshop.ToV2Dto();
        workshopV2Dto.Id = Guid.NewGuid(); // simulate update
        workshopV2Dto.InstitutionHierarchyId = institutionHierarchyId;
        workshopV2Dto.WorkshopType = WorkshopType.Workshop;

        var updateDto = new WorkshopDraftUpdateDto
        {
            Id = Guid.NewGuid(),
            WorkshopV2Dto = workshopV2Dto
        };

        var updatedDraft = new WorkshopDraft
        {
            Id = updateDto.Id,
            WorkshopDraftContent = new WorkshopDraftContent
            {
                InstitutionHierarchyId = institutionHierarchyId,
                IsChampionPath = false,
                WorkshopType = WorkshopType.Workshop
            },
            Teachers = new List<TeacherDraft>()
        };

        codeficatorRepositoryMoq.Setup(x => x.Get(It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsQueryable().BuildMock());

        institutionHierarchyRepositoryMoq.Setup(x => x.GetById(institutionHierarchyId))
            .ReturnsAsync(new InstitutionHierarchy
            {
                Id = institutionHierarchyId,
                Institution = new Institution { Title = "NotMinSport" }
            });

        languageServiceMoq.Setup(x => x.GetById(workshopV2Dto.LanguageOfEducationId))
            .ReturnsAsync(new LanguageDto { Id = workshopV2Dto.LanguageOfEducationId, Name = workshopV2Dto.LanguageOfEducationName });

        workshopDraftRepoMoq
            .Setup(x => x.GetById(updateDto.Id))
            .ReturnsAsync(updatedDraft);

        workshopDraftRepoMoq
            .Setup(x => x.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync(updatedDraft);

        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), true))
            .ReturnsAsync(workshopV2Dto);

        workshopDraftRepoMoq
            .Setup(x => x.RunInTransaction(It.IsAny<Func<Task<(WorkshopDraft, ImageChangingResult, MultipleImageChangingResult, List<TeacherCreateUpdateResultDto>)>>>()))
            .Returns<Func<Task<(WorkshopDraft, ImageChangingResult, MultipleImageChangingResult, List<TeacherCreateUpdateResultDto>)>>>(f => f());

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.WorkshopDraft.Should().NotBeNull();
        result.WorkshopDraft.WorkshopDetails.IsChampionPath.Should().BeFalse();
        result.WorkshopDraft.WorkshopDetails.WorkshopType.Should().Be(WorkshopType.Workshop);
    }

    #endregion

    #region Delete
    [Test]
    public async Task Delete_WhenEntityWithIdExists_ShouldTryToDelete()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();

        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Delete(It.IsAny<WorkshopDraft>()))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);

        // Act
        await service.Delete(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
    }
    #endregion

    #region SendForModeration
    [Test]
    public async Task SendForModeration_WhenEntityWithIdExists_ShouldTryToUpdate()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();

        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);

        // Act
        await service.SendForModeration(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
    }
    #endregion

    #region Approve
    [Test]
    public async Task Approve_WhenWorkshopIdIsNull_ShouldTryToCreateNewWorkshopAndDeleteDraft()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();
        workshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;
        workshopDraft.WorkshopId = null;

        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Delete(It.IsAny<WorkshopDraft>()))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);
        workshopServiceCombinerV2Moq.Setup(x => x.Create(It.IsAny<WorkshopV2CreateRequestDto>()))
            .Verifiable(Times.Once);

        // Act
        await service.Approve(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        workshopServiceCombinerV2Moq.VerifyAll();
    }

    [Test]
    public async Task Approve_WhenWorkshopIdIsNotNull_ShouldTryToUpdateExistingWorkshopAndDeleteDraft()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();
        workshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;   

        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Delete(It.IsAny<WorkshopDraft>()))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);
        workshopServiceCombinerV2Moq.Setup(x => x.Update(It.IsAny<WorkshopV2Dto>()))
            .Verifiable(Times.Once);

        // Act
        await service.Approve(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        workshopServiceCombinerV2Moq.VerifyAll();
    }
    #endregion

    #region Reject
    [Test]
    public async Task Reject_WhenWorkshopIdIsNotNull_ShouldTryToUpdateWorkshopDraft()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();
        workshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;

        var rejectionMessage = "rejectionMessage";

        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Update(It.IsAny<WorkshopDraft>()))
            .Verifiable(Times.Once);

        // Act
        await service.Reject(workshop.Id, rejectionMessage).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
    }

    [Test]
    public async Task Reject_WhenWorkshopHasWrongStatus_ShouldThrowArgumentException()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();
        workshopDraft.DraftStatus = WorkshopDraftStatus.Draft;

        var rejectionMessage = "rejectionMessage";

        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Update(It.IsAny<WorkshopDraft>()))
            .Verifiable(Times.Never);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await service.Reject(workshop.Id, rejectionMessage));

        workshopDraftRepoMoq.VerifyAll();
    }
    #endregion

    #region GetByProviderId
    [Test]
    public async Task GetByProviderId_WhenProviderHasNoDrafts_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<WorkshopDraft>();

        institutionHierarchyRepositoryMoq.Setup(
            x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<InstitutionHierarchy, object>>, SortDirection>>()))
            .Returns(new List<InstitutionHierarchy>().AsTestAsyncEnumerableQuery());
        workshopDraftRepoMoq.Setup(x => x.Count(It.IsAny<Expression<Func<WorkshopDraft, bool>>>()))
            .ReturnsAsync(0).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x =>
            x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(emptyList.AsTestAsyncEnumerableQuery).Verifiable(Times.Once);

        // Act
        var result = await service.GetByProviderId(Guid.NewGuid(), null).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
        result.Entities.Should().BeEmpty();
    }

    [Test]
    public async Task GetByProviderId_WhenProviderWithIdExists_ShouldReturnEntitiesWithCountedUnreadMessages()
    {
        // Arrange
        var numberOfWorkshops = 5;

        var workshops = WorkshopGenerator.Generate(numberOfWorkshops).WithProvider().WithTeachers();
        var workshopV2Dtos = workshops.ToV2Dto();
        var workshopDrafts = workshopV2Dtos.ToDraft();
        var workshopDraftResponses = workshopDrafts.ToCardDto();

        institutionHierarchyRepositoryMoq.Setup(
            x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<InstitutionHierarchy, object>>, SortDirection>>()))
            .Returns(new List<InstitutionHierarchy>().AsTestAsyncEnumerableQuery());
        workshopDraftRepoMoq.Setup(x => x.Count(It.IsAny<Expression<Func<WorkshopDraft, bool>>>()))
            .ReturnsAsync(numberOfWorkshops).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x =>
            x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDrafts.AsTestAsyncEnumerableQuery).Verifiable(Times.Once);

        // Act
        var result = await service.GetByProviderId(Guid.NewGuid(), null).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
        result.Entities.Should().BeEquivalentTo(workshopDraftResponses);
    }
    #endregion

    #region UpdateWorkshop
    [Test]
    public async Task UpdateWorkshop_WhenModeratedFieldsWasNotChanged_ShouldCallWorkshopUpdate()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopDto = workshop.ToDto();
        var workshopV2Dto = workshop.ToV2Dto();

        var workshopResultDto = new WorkshopResultDto
        {
            Workshop = workshopV2Dto
        };

        var workshopDrafts = new List<WorkshopDraft>();

        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(workshopDto).Verifiable(Times.Once);
        workshopServiceCombinerV2Moq.Setup(x => x.Update(It.IsAny<WorkshopV2Dto>()))
            .ReturnsAsync(Result<WorkshopResultDto>.Success(workshopResultDto)).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x =>
            x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDrafts.AsQueryable().BuildMock()).Verifiable(Times.Once);

        // Act
        var result = await service.UpdateWorkshop(workshopV2Dto).ConfigureAwait(false);

        // Assert
        currentUserServiceMoq.VerifyAll();
        workshopServiceCombinerV2Moq.VerifyAll();
        workshopDraftRepoMoq.VerifyAll();

        result.Should().NotBeNull();
    }

    [Test]
    public void UpdateWorkshop_WhenDraftExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopDto = workshop.ToDto();
        var workshopV2Dto = workshop.ToV2Dto();

        var workshopDrafts = new List<WorkshopDraft>()
        {
            new()
        };

        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(workshopDto).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x =>
           x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDrafts.AsQueryable().BuildMock()).Verifiable(Times.Once);

        //Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.UpdateWorkshop(workshopV2Dto));

        currentUserServiceMoq.VerifyAll();
        workshopServiceCombinerV2Moq.VerifyAll();
        workshopDraftRepoMoq.VerifyAll();        
    }

    [Test]
    public void UpdateWorkshop_WhenWorkshopIsNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopDto = (WorkshopDto)null;
        var workshopV2Dto = workshop.ToV2Dto();
        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(workshopDto).Verifiable(Times.Once);

        //Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.UpdateWorkshop(workshopV2Dto));

        workshopServiceCombinerV2Moq.VerifyAll();
    }

    [Test]
    public async Task UpdateWorkshop_WhenModeratedFieldsWasChanged_ShouldCallCreateDraft()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers().WithLanguage();
        var workshopDto = workshop.ToDto();
        workshopDto.Title = "Changed title";
        var workshopV2Dto = workshop.ToV2Dto();

        var workshopDrafts = new List<WorkshopDraft>();
        var workshopDraft = workshopV2Dto.ToDraft();
        
        institutionHierarchyRepositoryMoq.Setup(x => x.GetById(workshop.InstitutionHierarchyId.Value))
            .ReturnsAsync(new InstitutionHierarchy
            {
                Id = workshop.InstitutionHierarchyId.Value,
                Institution = new Institution { Title = "Мінспорт" },
            });
        languageServiceMoq.Setup(x => x.GetById(workshop.LanguageOfEducationId))
            .ReturnsAsync(new LanguageDto { Id = workshop.LanguageOfEducationId, Name = workshop.LanguageOfEducation.Name });

        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(workshopDto).Verifiable(Times.Exactly(2));
        workshopDraftRepoMoq.Setup(x =>
            x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDrafts.AsQueryable().BuildMock()).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.RunInTransaction(It.IsAny<Func<Task<WorkshopDraft>>>()))
            .ReturnsAsync(workshopDraft);
        codeficatorRepositoryMoq.Setup(x => x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsQueryable().BuildMock());

        // Act
        var result = await service.UpdateWorkshop(workshopV2Dto).ConfigureAwait(false);

        // Assert
        currentUserServiceMoq.VerifyAll();
        workshopServiceCombinerV2Moq.VerifyAll();
        workshopDraftRepoMoq.VerifyAll();

        result.Should().NotBeNull();
    }
    #endregion

    #region CreateDraftForReactivation

    [Test]
    public async Task CreateDraftForReactivation_WhenWorkshopIdIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        Guid workshopId = Guid.Empty;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await service.CreateDraftForReactivation(workshopId));
    }

    [Test]
    public async Task CreateDraftForReactivation_WhenWorkshopStatusIsNotClosed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto;

        var workshopDraft = new WorkshopDraft();

        var options = new Mock<IOptions<UploadConcurrencySettings>>();
        var settings = new UploadConcurrencySettings();
        options.Setup(o => o.Value).Returns(settings);

        var logger = new Mock<ILogger<WorkshopDraftService>>();
        var workshopDraftImagesService = new Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>>();
        var teacherDraftImagesService = new Mock<IEntityCoverImageInteractionService<TeacherDraft>>();
        var regionAdminService = new Mock<IRegionAdminService>();
        var ministryAdminService = new Mock<IMinistryAdminService>();
        var codeficatorService = new Mock<ICodeficatorService>();
        var searchStringService = new Mock<ISearchStringService>();

        workshopDraftRepoMoq.Setup(x => x.RunInTransaction(It.IsAny<Func<Task<WorkshopDraft>>>()))
            .ReturnsAsync(workshopDraft);
        workshopServiceCombinerV2Moq.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(workshopV2Dto).Verifiable(Times.Once);
        codeficatorRepositoryMoq.Setup(x => x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsQueryable().BuildMock());

        var service = new WorkshopDraftService(
                   logger.Object,
                   new Mock<ILanguageService>().Object,
                   workshopDraftRepoMoq.Object,
                   workshopDraftImagesService.Object,
                   providerServiceMoq.Object,
                   currentUserServiceMoq.Object,
                   teacherDraftImagesService.Object,
                   tagRepositoryMoq.Object,
                   options.Object,
                   workshopServiceCombinerV2Moq.Object,
                   regionAdminService.Object,
                   ministryAdminService.Object,
                   codeficatorService.Object,
                   searchStringService.Object,
                   institutionHierarchyRepositoryMoq.Object,
                   codeficatorRepositoryMoq.Object,
                   new Mock<IChangesLogService>().Object,
                   institutionOptionsMock.Object);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateDraftForReactivation(workshop.Id));

        //Assert 
        currentUserServiceMoq.VerifyAll();
    }

    #endregion

    #region GetWorkshopDraftIdByWorkshopId
    [Test]
    public async Task GetWorkshopDraftIdByWorkshopId_WhenWorkshopDraftExists_ShouldReturnId()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate();
        var workshopV2Dto = workshop.ToV2Dto();

        var workshopDrafts = new List<WorkshopDraft>()
        {
            workshopV2Dto.ToDraft()
        };

        workshopDraftRepoMoq.Setup(x =>
            x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDrafts.AsQueryable().BuildMock()).Verifiable(Times.Once);

        // Act
        var result = await service.GetWorkshopDraftIdByWorkshopId(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();

        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetWorkshopDraftIdByWorkshopId_WhenWorkshopDraftDoesntExist_ShouldReturnNull()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate();

        var workshopDrafts = new List<WorkshopDraft>();

        workshopDraftRepoMoq.Setup(x =>
            x.Get(It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDrafts.AsQueryable().BuildMock()).Verifiable(Times.Once);

        // Act
        var result = await service.GetWorkshopDraftIdByWorkshopId(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();

        result.Should().BeNull();
    }
    #endregion
    private void SetupInstitutionHierarchy()
    {
        institutionHierarchyServiceMock.Setup(s => s.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(new InstitutionHierarchyDto
            {
                Institution = new InstitutionDto { Title = "Мінспорт"}
            });
    }
}
