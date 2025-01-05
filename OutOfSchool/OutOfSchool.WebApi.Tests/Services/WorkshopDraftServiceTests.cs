using AutoMapper;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using System.Threading.Tasks;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.BusinessLogic.Models.Workshops;
using System;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using System.Linq.Expressions;
using System.Linq;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.Common.Extensions;
using FluentAssertions;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;
using System.Collections.Generic;
using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class WorkshopDraftServiceTests
{
    private IWorkshopDraftService service;  
    private Mock<IWorkshopDraftRepository> workshopDraftRepoMoq;
    private IMapper mapper;

    private Mock<IProviderService> providerServiceMoq;
    private Mock<ICurrentUserService> currentUserServiceMoq;
    private Mock<IEntityRepository<long, Tag>> tagRepositoryMoq;
    private Mock<IWorkshopServicesCombinerV2> workshopServiceCombinerV2Moq;

    private string userId;

    [SetUp]
    public void SetUp()
    {
        workshopDraftRepoMoq = new Mock<IWorkshopDraftRepository>();

        var config = new MapperConfiguration(cfg =>
            cfg.UseProfile<CommonProfile>()
               .UseProfile<MappingProfile>()
               .UseProfile<WorkshopDraftMappingProfile>());

        mapper = config.CreateMapper();

        currentUserServiceMoq = new Mock<ICurrentUserService>();
        providerServiceMoq = new Mock<IProviderService>();
        tagRepositoryMoq = new Mock<IEntityRepository<long, Tag>>();
        workshopServiceCombinerV2Moq = new Mock<IWorkshopServicesCombinerV2>();

        var options = new Mock<IOptions<UploadConcurrencySettings>>();
        var settings = new UploadConcurrencySettings();
        options.Setup(o => o.Value).Returns(settings);

        var logger = new Mock<ILogger<WorkshopDraftService>>();
        var workshopDraftImagesService = new Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>>();   
        var teacherDraftImagesService = new Mock<IEntityCoverImageInteractionService<TeacherDraft>>();       
        var employeeService = new Mock<IEmployeeService>();        

        userId = "someUserId";

        service = new WorkshopDraftService(
                   logger.Object,
                   workshopDraftRepoMoq.Object,
                   mapper,
                   workshopDraftImagesService.Object,
                   providerServiceMoq.Object,
                   currentUserServiceMoq.Object,
                   teacherDraftImagesService.Object,
                   tagRepositoryMoq.Object,
                   options.Object,
                   employeeService.Object,
                   workshopServiceCombinerV2Moq.Object);
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
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);

        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);
        var workshopResponse= mapper.Map<WorkshopDraftResponseDto>(workshopDraft);
        workshopResponse.Tags = [];

        var providerDto = mapper.Map<ProviderDto>(workshop.Provider);
        providerDto.UserId = userId;

        currentUserServiceMoq.Setup(x => x.UserId)
            .Returns(userId).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providerDto).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.RunInTransaction(It.IsAny<Func<Task<WorkshopDraft>>>()))
            .ReturnsAsync(workshopDraft);
        tagRepositoryMoq.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Tag, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<Tag>()).Verifiable(Times.Once);
        
        // Act 
        var result = await service.Create(workshopV2Dto).ConfigureAwait(false);

        //Assert 
        currentUserServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();
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
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);
        var workshopResponse = mapper.Map<WorkshopDraftResponseDto>(workshopDraft);

        var workshopUpdateDto = new WorkshopDraftUpdateDto()
        {
            Id = Guid.NewGuid(),
            WorkshopV2Dto = workshopV2Dto
        };

        var providerDto = mapper.Map<ProviderDto>(workshop.Provider);
        providerDto.UserId = userId;        

        currentUserServiceMoq.Setup(x => x.UserId)
            .Returns(userId).Verifiable(Times.Exactly(2));
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providerDto).Verifiable(Times.Exactly(2));
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

        //Act
        var result = await service.Update(workshopUpdateDto).ConfigureAwait(false);

        //Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();           

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
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);

        var providerDto = mapper.Map<ProviderDto>(workshop.Provider);
        providerDto.UserId = userId;

        currentUserServiceMoq.Setup(x => x.UserId)
            .Returns(userId).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providerDto).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Delete(It.IsAny<WorkshopDraft>()))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);

        // Act
        await service.Delete(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();
    }
    #endregion

    #region SendForModeration
    [Test]
    public async Task SendForModeration_WhenEntityWithIdExists_ShouldTryToUpdate()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);

        var providerDto = mapper.Map<ProviderDto>(workshop.Provider);
        providerDto.UserId = userId;

        currentUserServiceMoq.Setup(x => x.UserId)
            .Returns(userId).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providerDto).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);

        // Act
        await service.SendForModeration(workshop.Id).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();
    }
    #endregion

    #region Approve
    [Test]
    public async Task Approve_WhenWorkshopIdIsNull_ShouldTryToCreateNewWorkshopAndDeleteDraft()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);
        workshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;
        workshopDraft.WorkshopId = null;

        var providerDto = mapper.Map<ProviderDto>(workshop.Provider);
        providerDto.UserId = userId;

        currentUserServiceMoq.Setup(x => x.UserId)
            .Returns(userId).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providerDto).Verifiable(Times.Once);
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
        currentUserServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();
        workshopServiceCombinerV2Moq.VerifyAll();
    }

    [Test]
    public async Task Approve_WhenWorkshopIdIsNotNull_ShouldTryToUpdateExistingWorkshopAndDeleteDraft()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);
        workshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;        

        var providerDto = mapper.Map<ProviderDto>(workshop.Provider);
        providerDto.UserId = userId;

        currentUserServiceMoq.Setup(x => x.UserId)
            .Returns(userId).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providerDto).Verifiable(Times.Once);
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
        currentUserServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();
        workshopServiceCombinerV2Moq.VerifyAll();
    }
    #endregion

    #region Reject
    [Test]
    public async Task Reject_WhenWorkshopIdIsNotNull_ShouldTryToUpdateWorkshopDraft()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);
        workshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;

        var providerDto = mapper.Map<ProviderDto>(workshop.Provider);
        providerDto.UserId = userId;

        var rejectionMessage = "rejectionMessage";

        currentUserServiceMoq.Setup(x => x.UserId)
            .Returns(userId).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providerDto).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Update(It.IsAny<WorkshopDraft>()))
            .Verifiable(Times.Once);

        // Act
        await service.Reject(workshop.Id, rejectionMessage).ConfigureAwait(false);

        // Assert
        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();
    }

    [Test]
    public async Task Reject_WhenWorkshopHasWrongStatus_ShouldThrowArgumentException()
    {
        // Arrange
        var workshop = WorkshopGenerator.Generate().WithProvider().WithTeachers();
        var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshop);
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);
        workshopDraft.DraftStatus = WorkshopDraftStatus.Draft;

        var providerDto = mapper.Map<ProviderDto>(workshop.Provider);
        providerDto.UserId = userId;

        var rejectionMessage = "rejectionMessage";

        currentUserServiceMoq.Setup(x => x.UserId)
            .Returns(userId).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(providerDto).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(workshopDraft).Verifiable(Times.Once);
        workshopDraftRepoMoq.Setup(x => x.Update(It.IsAny<WorkshopDraft>()))
            .Verifiable(Times.Never);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await service.Reject(workshop.Id, rejectionMessage));

        workshopDraftRepoMoq.VerifyAll();
        currentUserServiceMoq.VerifyAll();
        providerServiceMoq.VerifyAll();
    }
    #endregion
}
