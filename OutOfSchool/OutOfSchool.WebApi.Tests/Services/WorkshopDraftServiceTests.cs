using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
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

    private Mock<IProviderService> providerServiceMoq;
    private Mock<ICurrentUserService> currentUserServiceMoq;
    private Mock<IEntityRepository<long, Tag>> tagRepositoryMoq;
    private Mock<IWorkshopServicesCombinerV2> workshopServiceCombinerV2Moq;
    private Mock<IInstitutionHierarchyRepository> institutionHierarchyRepositoryMoq;
    private Mock<ICodeficatorRepository> codeficatorRepositoryMoq;
    private Mock<IChangesLogService> changesLogServiceMock;

    private string userId;

    [SetUp]
    public void SetUp()
    {
        workshopDraftRepoMoq = new Mock<IWorkshopDraftRepository>();

        currentUserServiceMoq = new Mock<ICurrentUserService>();
        providerServiceMoq = new Mock<IProviderService>();
        tagRepositoryMoq = new Mock<IEntityRepository<long, Tag>>();
        workshopServiceCombinerV2Moq = new Mock<IWorkshopServicesCombinerV2>();
        institutionHierarchyRepositoryMoq = new Mock<IInstitutionHierarchyRepository>();
        codeficatorRepositoryMoq = new Mock<ICodeficatorRepository>();
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

        userId = "someUserId";

        service = new WorkshopDraftService(
                   logger.Object,
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
                   changesLogServiceMock.Object);
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
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();    
        var workshopV2Dto = workshop.ToV2Dto();

        var workshopDraft = workshopV2Dto.ToDraft();
        var workshopResponse = workshopDraft.ToResponseDto();

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
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = workshop.ToV2Dto();
        var workshopDraft = workshopV2Dto.ToDraft();
        var workshopResponse = workshopDraft.ToResponseDto();

        var workshopUpdateDto = new WorkshopDraftUpdateDto()
        {
            Id = Guid.NewGuid(),
            WorkshopV2Dto = workshopV2Dto
        };

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
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopDto = workshop.ToDto();
        workshopDto.Title = "Changed title";
        var workshopV2Dto = workshop.ToV2Dto();

        var workshopDrafts = new List<WorkshopDraft>();
        var workshopDraft = workshopV2Dto.ToDraft();

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
}
