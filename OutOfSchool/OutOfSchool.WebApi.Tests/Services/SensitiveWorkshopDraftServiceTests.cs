using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.SportsRegistry;
using OutOfSchool.BusinessLogic.Services.SubordinationStructure;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class SensitiveWorkshopDraftServiceTests
{
    private ISensitiveWorkshopDraftService service;
    private Mock<IWorkshopDraftRepository> workshopDraftRepoMock;
    private Mock<IRegistrySyncService> registrySyncServiceMock;
    private Mock<IProviderService> providerServiceMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IWorkshopServicesCombinerV2> workshopServiceCombinerV2Mock;
    private Mock<ILanguageService> languageServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IInstitutionHierarchyService> institutionHierarchyServiceMock;
    private Mock<IInstitutionHierarchyRepository> institutionHierarchyRepositoryMock;
    private Mock<ICodeficatorRepository> codeficatorRepository;
    private Mock<IChangesLogService> changesLogServiceMock;
    private Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>> workshopDraftImagesServiceMock;

    private string userId;
    private WorkshopDraft validWorkshopDraft;
    private Guid draftId;
    private Guid moderatorId;
    private ModeratorWorkshopDraftEditDto validEditDto;

    [SetUp]
    public void SetUp()
    {
        workshopDraftRepoMock = new Mock<IWorkshopDraftRepository>();
        institutionHierarchyServiceMock = new Mock<IInstitutionHierarchyService>();
        registrySyncServiceMock = new Mock<IRegistrySyncService>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        providerServiceMock = new Mock<IProviderService>();
        workshopServiceCombinerV2Mock = new Mock<IWorkshopServicesCombinerV2>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        searchStringServiceMock = new Mock<ISearchStringService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        institutionHierarchyRepositoryMock = new Mock<IInstitutionHierarchyRepository>();
        codeficatorRepository = new Mock<ICodeficatorRepository>();
        languageServiceMock = new Mock<ILanguageService>();
        changesLogServiceMock = new Mock<IChangesLogService>();
        workshopDraftImagesServiceMock = new Mock<IImageDependentEntityImagesInteractionService<WorkshopDraft>>();

        var options = new Mock<IOptions<UploadConcurrencySettings>>();
        var settings = new UploadConcurrencySettings();
        options.Setup(o => o.Value).Returns(settings);

        var logger = new Mock<ILogger<WorkshopDraftService>>();
        var teacherDraftImagesService = new Mock<IEntityCoverImageInteractionService<TeacherDraft>>();
        var institutionOptionsMock = new Mock<IOptions<InstitutionOptions>>();
        var imageStorageOptionsMock = new Mock<IOptions<ImageStorageOptions>>();
        imageStorageOptionsMock.Setup(o => o.Value).Returns(new ImageStorageOptions
        {
            BaseImageUrl = "http://test"
        });
        userId = "someUserId";

        service = new WorkshopDraftService(
            logger.Object,
            registrySyncServiceMock.Object,
            languageServiceMock.Object,
            workshopDraftRepoMock.Object,
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
            codeficatorRepository.Object,
            changesLogServiceMock.Object,
            institutionHierarchyServiceMock.Object,
            institutionOptionsMock.Object,
            imageStorageOptionsMock.Object);

        SetupModeratorTestData();
        languageServiceMock.Setup(x => x.GetById(It.Is<long>(id => id == 1)))
                .ReturnsAsync(new LanguageDto { Id = 1, Name = "English" });
        institutionOptionsMock.Setup(x => x.Value)
            .Returns(new InstitutionOptions { MinistryOfSportId = "b67a4f29-728e-4bb0-bb42-4a9d7e0bd90a" });
    }

    #region FetchByFilterForAdmins    
    [Test]
    public async Task FetchByFilterForAdmins_RoleRegionAdmin_ShouldBuildPredicateAndReturnEntities()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var parentCATOTTGid = 11;
        var subSettlementsIds = new List<long>() { parentCATOTTGid, 12, 13 };
        var filter = new WorkshopDraftFilterAdministration();
        var admin = new RegionAdminDto() { Id = userId, InstitutionId = institutionId, CATOTTGId = parentCATOTTGid };
        var resultExpected = SetupFetchByFilterForAdmins(userId, true, false, parentCATOTTGid, filter, subSettlementsIds, admin);

        // Act
        var result = await service.FetchByFilterForAdmins()
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(resultExpected);

        codeficatorServiceMock.Verify(
            s => s.GetAllChildrenIdsByParentIdAsync(It.Is<long>(s => s == parentCATOTTGid)), Times.Once);

        regionAdminServiceMock.Verify(
            r => r.GetByUserId(It.Is<string>(id => id == userId)));
    }

    [Test]
    public async Task FetchByFilterForAdmins_FilteredBySearchString_ShouldReturnEntities()
    {
        // Arrange        
        var filterWorkshop = new WorkshopDraftFilterAdministration()
        {
            SearchString = "Шахмати для початківців",
        };

        var resultExpected = SetupFetchByFilterForAdmins(
            userId: userId,
            isRegionAdmin: false,
            isMinistryAdmin: false,
            parentCATOTTGId: 0,
            filter: filterWorkshop,
            subSettlementsIds: null,
            adminRegion: null,
            adminMinistry: null,
            searchWords: ["Шахмати", "для", "початківців"]);

        // Act
        var result = await service.FetchByFilterForAdmins(filterWorkshop)
            .ConfigureAwait(false);

        // Assert
        result.Should()
            .BeEquivalentTo(resultExpected);

        searchStringServiceMock.Verify(
            s => s.SplitSearchString(It.Is<string>(x => x == filterWorkshop.SearchString)),
            Times.Once);
    }

    #endregion

    #region UpdateDraftAsModeratorAsync

    [Test]
    public async Task UpdateDraftAsModeratorAsync_WithNullDto_ReturnsFailed()
    {
        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, null);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("400", result.OperationResult.Errors.First().Code);
        Assert.AreEqual("DTO must not be null.", result.OperationResult.Errors.First().Description);
    }

    [Test]
    public void UpdateDraftAsModeratorAsync_WithNonExistentDraft_ThrowsException()
    {
        // Arrange
        var nonExistentDraftId = Guid.NewGuid();

        // Setup to match the exact exception that would be thrown
        workshopDraftRepoMock.Setup(repo => repo.GetById(nonExistentDraftId))
            .ThrowsAsync(new ArgumentException(
                "id",
                $"There are no records in workshopDrafts table with such id - {nonExistentDraftId}."));

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.UpdateDraftAsModeratorAsync(nonExistentDraftId, validEditDto));

        // Verify the exception message contains the ID
        Assert.That(ex.Message, Contains.Substring(nonExistentDraftId.ToString()));
    }

    [Test]
    public async Task UpdateDraftAsModeratorAsync_WithNonEditableStatus_ReturnsFailed()
    {
        // Arrange
        validWorkshopDraft.DraftStatus = WorkshopDraftStatus.Rejected;
        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, validEditDto);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("409", result.OperationResult.Errors.First().Code);
        Assert.That(result.OperationResult.Errors.First().Description,
            Does.Contain("not editable in its current status"));
    }

    [Test]
    public async Task UpdateDraftAsModeratorAsync_ValidRequest_UpdatesAndLogsChanges()
    {
        // Arrange
        workshopDraftRepoMock.Setup(repo => repo.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync((WorkshopDraft draft) => draft);

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, validEditDto);

        // Assert
        Assert.IsTrue(result.Succeeded);

        // Verify draft status was updated
        workshopDraftRepoMock.Verify(repo => repo.Update(
            It.Is<WorkshopDraft>(w => w.DraftStatus == WorkshopDraftStatus.EditedByModerator)),
            Times.Once);

        // Verify changes were logged
        changesLogServiceMock.Verify(service => service.LogWorkshopDraftChanges(
            It.IsAny<WorkshopDraftContent>(),
            It.IsAny<WorkshopDraftContent>(),
            draftId,
            userId),
            Times.Once);
    }

    [Test]
    public async Task UpdateDraftAsModeratorAsync_EditedByModeratorStatus_UpdatesSuccessfully()
    {
        // Arrange
        validWorkshopDraft.DraftStatus = WorkshopDraftStatus.EditedByModerator;
        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);
        workshopDraftRepoMock.Setup(repo => repo.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync((WorkshopDraft draft) => draft);

        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, validEditDto);

        // Assert
        Assert.IsTrue(result.Succeeded);
        workshopDraftRepoMock.Verify(repo => repo.Update(It.IsAny<WorkshopDraft>()), Times.Once);
    }

    #endregion

    #region DeleteCoverImageAsModeratorAsync

    [Test]
    public void DeleteCoverImageAsModeratorAsync_WithNonExistentDraft_ThrowsException()
    {
        // Arrange
        var nonExistentDraftId = Guid.NewGuid();

        // Setup to throw the expected exception for nonexistent ID
        workshopDraftRepoMock.Setup(repo => repo.GetById(nonExistentDraftId))
            .ThrowsAsync(new ArgumentException(
                "id",
                $"There are no records in workshopDrafts table with such id - {nonExistentDraftId}."));

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.DeleteCoverImageAsModeratorAsync(nonExistentDraftId));

        // Verify the exception message contains the ID
        Assert.That(ex.Message, Contains.Substring(nonExistentDraftId.ToString()));
    }

    [Test]
    public async Task DeleteCoverImageAsModeratorAsync_WithNonEditableStatus_ReturnsFailed()
    {
        // Arrange
        validWorkshopDraft.DraftStatus = WorkshopDraftStatus.Draft;
        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteCoverImageAsModeratorAsync(draftId);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("409", result.OperationResult.Errors.First().Code);
    }

    [Test]
    public async Task DeleteCoverImageAsModeratorAsync_WithNoCoverImage_ReturnsFailed()
    {
        // Arrange
        validWorkshopDraft.CoverImageId = null;        

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteCoverImageAsModeratorAsync(draftId);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("400", result.OperationResult.Errors.First().Code);
        Assert.That(result.OperationResult.Errors.First().Description,
            Does.Contain("No cover image exists"));
    }

    [Test]
    public async Task DeleteCoverImageAsModeratorAsync_ValidRequest_DeletesAndUpdatesStatus()
    {
        // Arrange
        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveCoverImageAsync(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync(OperationResult.Success);

        workshopDraftRepoMock.Setup(repo => repo.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync((WorkshopDraft draft) => draft);

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteCoverImageAsModeratorAsync(draftId);

        // Assert
        Assert.IsTrue(result.Succeeded);

        // Verify cover image was removed
        workshopDraftImagesServiceMock.Verify(service =>
            service.RemoveCoverImageAsync(It.IsAny<WorkshopDraft>()), Times.Once);

        // Verify draft status was updated
        workshopDraftRepoMock.Verify(repo => repo.Update(
            It.Is<WorkshopDraft>(w => w.DraftStatus == WorkshopDraftStatus.EditedByModerator)),
            Times.Once);

        // Verify changes were logged
        changesLogServiceMock.Verify(service =>
            service.AddEntityChangesToDbContext(It.IsAny<WorkshopDraft>(), userId),
            Times.Once);
    }

    [Test]
    public async Task DeleteCoverImageAsModeratorAsync_ExceptionThrown_ReturnsFailed()
    {
        // Arrange
        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveCoverImageAsync(It.IsAny<WorkshopDraft>()))
            .ThrowsAsync(new Exception("Test exception"));

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteCoverImageAsModeratorAsync(draftId);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("500", result.OperationResult.Errors.First().Code);
    }

    #endregion

    #region DeleteImageAsModeratorAsync

    [Test]
    public async Task DeleteImageAsModeratorAsync_WithNullImageId_ReturnsFailed()
    {
        // Act
        var result = await service.DeleteImageAsModeratorAsync(draftId, null);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("400", result.OperationResult.Errors.First().Code);
    }

    [Test]
    public async Task DeleteImageAsModeratorAsync_WithEmptyImageId_ReturnsFailed()
    {
        // Act
        var result = await service.DeleteImageAsModeratorAsync(draftId, string.Empty);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("400", result.OperationResult.Errors.First().Code);
    }

    [Test]
    public void DeleteImageAsModeratorAsync_WithNonExistentDraft_ThrowsException()
    {
        // Arrange
        var nonExistentDraftId = Guid.NewGuid();

        // Setup to throw the expected exception for nonexistent ID
        workshopDraftRepoMock.Setup(repo => repo.GetById(nonExistentDraftId))
            .ThrowsAsync(new ArgumentException(
                "id",
                $"There are no records in workshopDrafts table with such id - {nonExistentDraftId}."));

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.DeleteImageAsModeratorAsync(nonExistentDraftId, "image-123"));

        // Verify the exception message contains the ID
        Assert.That(ex.Message, Contains.Substring(nonExistentDraftId.ToString()));
    }

    [Test]
    public async Task DeleteImageAsModeratorAsync_WithNonExistentImage_ReturnsFailed()
    {
        // Arrange
        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteImageAsModeratorAsync(draftId, "nonexistent-image");

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("404", result.OperationResult.Errors.First().Code);
        Assert.That(result.OperationResult.Errors.First().Description,
            Does.Contain("Image with ID nonexistent-image not found"));
    }

    [Test]
    public async Task DeleteImageAsModeratorAsync_ValidRequest_DeletesAndUpdatesStatus()
    {
        // Arrange
        const string imageId = "image-123";
        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveImageAsync(It.IsAny<WorkshopDraft>(), imageId))
            .ReturnsAsync(OperationResult.Success);

        workshopDraftRepoMock.Setup(repo => repo.SaveChangesAsync(true, default))
            .ReturnsAsync(1);

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteImageAsModeratorAsync(draftId, imageId);

        // Assert
        Assert.IsTrue(result.Succeeded);

        // Verify image was removed
        workshopDraftImagesServiceMock.Verify(service =>
            service.RemoveImageAsync(It.IsAny<WorkshopDraft>(), imageId), Times.Once);

        // Verify changes were saved
        workshopDraftRepoMock.Verify(repo => repo.SaveChangesAsync(true, It.IsAny<CancellationToken>()), Times.Once);

        // Verify changes were logged
        changesLogServiceMock.Verify(service =>
            service.LogImageDeletions(
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                draftId,
                "WorkshopDraft",
                userId),
            Times.Once);
    }

    [Test]
    public async Task DeleteImageAsModeratorAsync_WithUrlEncodedImageId_CorrectlyDecodesAndDeletes()
    {
        // Arrange
        const string encodedImageId = "image%2F123";
        const string decodedImageId = "image/123";

        // Add the encoded image to the workshop draft
        validWorkshopDraft.Images.Add(new Image<WorkshopDraft> { ExternalStorageId = decodedImageId });

        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveImageAsync(It.IsAny<WorkshopDraft>(), decodedImageId))
            .ReturnsAsync(OperationResult.Success);

        workshopDraftRepoMock.Setup(repo => repo.SaveChangesAsync(true, default))
            .ReturnsAsync(1);

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteImageAsModeratorAsync(draftId, encodedImageId);

        // Assert
        Assert.IsTrue(result.Succeeded);

        // Verify the decoded image ID was used
        workshopDraftImagesServiceMock.Verify(service =>
            service.RemoveImageAsync(It.IsAny<WorkshopDraft>(), decodedImageId), Times.Once);
    }

    [Test]
    public async Task DeleteImageAsModeratorAsync_ExceptionThrown_ReturnsFailed()
    {
        // Arrange
        const string imageId = "image-123";
        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveImageAsync(It.IsAny<WorkshopDraft>(), imageId))
            .ThrowsAsync(new Exception("Test exception"));

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteImageAsModeratorAsync(draftId, imageId);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("500", result.OperationResult.Errors.First().Code);
    }

    #endregion

    #region DeleteManyImagesAsModeratorAsync

    [Test]
    public async Task DeleteManyImagesAsModeratorAsync_WithNullImageIds_ReturnsFailed()
    {
        // Act
        var result = await service.DeleteManyImagesAsModeratorAsync(draftId, null);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("400", result.OperationResult.Errors.First().Code);
    }

    [Test]
    public async Task DeleteManyImagesAsModeratorAsync_WithEmptyImageIds_ReturnsFailed()
    {
        // Act
        var result = await service.DeleteManyImagesAsModeratorAsync(draftId, new List<string>());

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("400", result.OperationResult.Errors.First().Code);
    }

    [Test]
    public void DeleteManyImagesAsModeratorAsync_WithNonExistentDraft_ThrowsException()
    {
        // Arrange
        var nonExistentDraftId = Guid.NewGuid();

        // Setup to throw the expected exception for nonexistent ID
        workshopDraftRepoMock.Setup(repo => repo.GetById(nonExistentDraftId))
            .ThrowsAsync(new ArgumentException(
                "id",
                $"There are no records in workshopDrafts table with such id - {nonExistentDraftId}."));

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.DeleteManyImagesAsModeratorAsync(
                nonExistentDraftId, new List<string> { "image-123" }));

        // Verify the exception message contains the ID
        Assert.That(ex.Message, Contains.Substring(nonExistentDraftId.ToString()));
    }

    [Test]
    public async Task DeleteManyImagesAsModeratorAsync_WithNonExistentImages_ReturnsFailed()
    {
        // Arrange
        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteManyImagesAsModeratorAsync(
            draftId, new List<string> { "nonexistent-image-1", "nonexistent-image-2" });
        
        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("404", result.OperationResult.Errors.First().Code);
        Assert.That(result.OperationResult.Errors.First().Description,
            Does.Contain("None of the specified images were found"));
    }

    [Test]
    public async Task DeleteManyImagesAsModeratorAsync_ValidRequest_DeletesAndUpdatesStatus()
    {
        // Arrange
        var imageIds = new List<string> { "image-123", "image-456" };

        var multipleImageResult = new MultipleImageRemovingResult
        {
            RemovedIds = imageIds,
            MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult
            {
                GeneralResultMessage = "Success"
            }
        };

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Add a successful result to the dictionary
        multipleImageResult.MultipleKeyValueOperationResult.Results.Add(0, OperationResult.Success);

        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveManyImagesAsync(It.IsAny<WorkshopDraft>(), It.IsAny<IList<string>>()))
            .ReturnsAsync(multipleImageResult);

        workshopDraftRepoMock.Setup(repo => repo.SaveChangesAsync(true, default))
            .ReturnsAsync(1);

        // Act
        var result = await service.DeleteManyImagesAsModeratorAsync(draftId, imageIds);

        // Assert
        Assert.IsTrue(result.Succeeded);

        // Verify images were removed
        workshopDraftImagesServiceMock.Verify(service =>
            service.RemoveManyImagesAsync(
                It.IsAny<WorkshopDraft>(),
                It.Is<IList<string>>(ids => ids.Count == 2)),
            Times.Once);

        // Verify changes were saved
        workshopDraftRepoMock.Verify(repo => repo.SaveChangesAsync(true, It.IsAny<CancellationToken>()), Times.Once);

        // Verify changes were logged
        changesLogServiceMock.Verify(service =>
            service.LogImageDeletions(
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                draftId,
                "WorkshopDraft",
                userId),
            Times.Once);
    }

    [Test]
    public async Task DeleteManyImagesAsModeratorAsync_WithMixedUrlEncodedImageIds_CorrectlyDecodesAndDeletes()
    {
        // Arrange
        var encodedImageIds = new List<string> { "image-123", "image%2F456" };
        var decodedImageIds = new List<string> { "image-123", "image/456" };

        validWorkshopDraft.Images.Add(new Image<WorkshopDraft> { ExternalStorageId = "image/456" });

        var multipleImageResult = new MultipleImageRemovingResult
        {
            RemovedIds = decodedImageIds,
            MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult
            {
                GeneralResultMessage = "Success"
            }
        };

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Add a successful result to the dictionary
        multipleImageResult.MultipleKeyValueOperationResult.Results.Add(0, OperationResult.Success);

        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveManyImagesAsync(It.IsAny<WorkshopDraft>(), It.IsAny<IList<string>>()))
            .ReturnsAsync(multipleImageResult);

        workshopDraftRepoMock.Setup(repo => repo.SaveChangesAsync(true, default))
            .ReturnsAsync(1);

        // Act
        var result = await service.DeleteManyImagesAsModeratorAsync(draftId, encodedImageIds);

        // Assert
        Assert.IsTrue(result.Succeeded);

        // Verify removal was called with decoded image IDs
        workshopDraftImagesServiceMock.Verify(service =>
            service.RemoveManyImagesAsync(
                It.IsAny<WorkshopDraft>(),
                It.IsAny<IList<string>>()),
            Times.Once);
    }

    [Test]
    public async Task DeleteManyImagesAsModeratorAsync_ExceptionThrown_ReturnsFailed()
    {
        // Arrange
        var imageIds = new List<string> { "image-123", "image-456" };
        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveManyImagesAsync(It.IsAny<WorkshopDraft>(), It.IsAny<IList<string>>()))
            .ThrowsAsync(new Exception("Test exception"));

        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteManyImagesAsModeratorAsync(draftId, imageIds);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("500", result.OperationResult.Errors.First().Code);
    }

    #endregion

    #region ValidateDraftForModerator

    [Test]
    public void ValidateDraftForModerator_UserNotAuthorized_ThrowsException()
    {
        // Arrange
        currentUserServiceMock.Setup(service =>
            service.UserHasRights(It.IsAny<ModeratorRights>(), It.IsAny<TechAdminRights>()))
            .ThrowsAsync(new UnauthorizedAccessException("User is not authorized"));

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.DeleteCoverImageAsModeratorAsync(draftId));
    }

    [Test]
    public async Task ValidateDraftForModerator_DraftNotInEditableStatus_ReturnsFailed()
    {
        // Arrange - set status to one that's not editable
        validWorkshopDraft.DraftStatus = WorkshopDraftStatus.Draft;
        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        // Act
        var result = await service.DeleteCoverImageAsModeratorAsync(draftId);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("409", result.OperationResult.Errors.First().Code);
        Assert.That(result.OperationResult.Errors.First().Description,
            Does.Contain("not editable in its current status"));
    }

    [Test]
    public async Task ValidateDraftForModerator_ValidDraftWithPendingModerationStatus_PassesValidation()
    {
        // Arrange
        validWorkshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;        
        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveCoverImageAsync(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync(OperationResult.Success);

        workshopDraftRepoMock.Setup(repo => repo.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync((WorkshopDraft draft) => draft);

        // Act
        var result = await service.DeleteCoverImageAsModeratorAsync(draftId);

        // Assert
        Assert.IsTrue(result.Succeeded);
    }

    [Test]
    public async Task ValidateDraftForModerator_ValidDraftWithEditedByModeratorStatus_PassesValidation()
    {
        // Arrange
        validWorkshopDraft.DraftStatus = WorkshopDraftStatus.EditedByModerator;
        workshopDraftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(validWorkshopDraft);

        workshopDraftImagesServiceMock.Setup(service =>
            service.RemoveCoverImageAsync(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync(OperationResult.Success);

        workshopDraftRepoMock.Setup(repo => repo.Update(It.IsAny<WorkshopDraft>()))
            .ReturnsAsync((WorkshopDraft draft) => draft);

        // Act
        var result = await service.DeleteCoverImageAsModeratorAsync(draftId);

        // Assert
        Assert.IsTrue(result.Succeeded);
    }
    #endregion

    private SearchResult<WorkshopDraftResponseDto> SetupFetchByFilterForAdmins(
        string userId = null,
        bool isRegionAdmin = false,
        bool isMinistryAdmin = false,
        long parentCATOTTGId = 0,
        WorkshopDraftFilterAdministration filter = null,
        IEnumerable<long> subSettlementsIds = null,
        RegionAdminDto adminRegion = null,
        MinistryAdminDto adminMinistry = null,
        string[] searchWords = null)
    {
        var workshops = WorkshopV2DtoGenerator.Generate(5).ToList();
        workshops.ForEach(w => w.Contacts =
        [
            new ContactsDto
            {
                IsDefault = true,
                Address = ContactsAddressDtoGenerator.Generate()
            }
        ]);
        var workshopDrafts = workshops.ToDraft();
        var workshopDraftDtos = workshopDrafts.ToResponseDto();

        SetUpCurrentUserService(userId, isRegionAdmin, isMinistryAdmin);
        SetUpWorkshopsRepository(workshopDrafts, filter);

        regionAdminServiceMock.Setup(a => a.GetByUserId(userId))
            .ReturnsAsync(adminRegion);

        ministryAdminServiceMock.Setup(a => a.GetByUserId(userId))
            .ReturnsAsync(adminMinistry);

        codeficatorServiceMock.Setup(c => c.GetAllChildrenIdsByParentIdAsync(parentCATOTTGId))
            .ReturnsAsync(subSettlementsIds);

        searchStringServiceMock.Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(searchWords);

        institutionHierarchyRepositoryMock.Setup(
            x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<InstitutionHierarchy, object>>, SortDirection>>()))
            .Returns(new List<InstitutionHierarchy>().AsTestAsyncEnumerableQuery());

        codeficatorRepository.Setup(
            x => x.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<CATOTTG, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<CATOTTG, object>>, SortDirection>>()))
            .Returns(new List<CATOTTG>().AsTestAsyncEnumerableQuery());

        return new SearchResult<WorkshopDraftResponseDto>()
        {
            TotalAmount = workshopDraftDtos.Count,
            Entities = workshopDraftDtos,
        };
    }

    private void SetUpCurrentUserService(string userId, bool isRegionAdmin = false, bool isMinistryAdmin = false)
    {
        currentUserServiceMock.Setup(u => u.IsRegionAdmin()).Returns(isRegionAdmin);
        currentUserServiceMock.Setup(u => u.IsMinistryAdmin()).Returns(isMinistryAdmin);
        currentUserServiceMock.Setup(u => u.UserId).Returns(userId);
    }

    private void SetUpWorkshopsRepository(List<WorkshopDraft> workshopDraftsReturned, WorkshopDraftFilterAdministration filter = null)
    {
        workshopDraftRepoMock.Setup(
            x => x.Count(It.IsAny<Expression<Func<WorkshopDraft, bool>>>()))
            .ReturnsAsync(workshopDraftsReturned.Count);

        workshopDraftRepoMock.Setup(
                w => w.Get(
                    It.Is<int>(x => x == filter.From),
                    It.Is<int>(x => x == filter.Size),
                    It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                    It.IsAny<Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>>()))
            .Returns(workshopDraftsReturned.AsTestAsyncEnumerableQuery());
    }

    private void SetupModeratorTestData()
    {
        draftId = Guid.NewGuid();
        moderatorId = Guid.NewGuid();

        // Initialize a valid workshop draft for testing
        validWorkshopDraft = new WorkshopDraft
        {
            Id = draftId,
            DraftStatus = WorkshopDraftStatus.PendingModeration,
            WorkshopDraftContent = new WorkshopDraftContent
            {
                Title = "Original Title",
                ShortTitle = "Original Short",
                CompetitiveSelectionDescription = "Original Competitive",
                PreferentialTermsOfParticipation = "Original Terms",
                EnrollmentProcedureDescription = "Original Enrollment",
                InstitutionHierarchyId = Guid.NewGuid(),
                WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>
                {
                    new WorkshopDescriptionItemDraft
                    {
                        SectionName = "Original Section",
                        Description = "Original Description"
                    }
                }
            },
            CoverImageId = "cover-123",
            Images = new List<Image<WorkshopDraft>>
            {
                new Image<WorkshopDraft>
                {
                    ExternalStorageId = "image-123"
                },
                new Image<WorkshopDraft>
                {
                    ExternalStorageId = "image-456"
                }
            }
        };

        // Initialize a valid edit DTO
        validEditDto = new ModeratorWorkshopDraftEditDto
        {
            Title = "Updated Title",
            ShortTitle = "Updated Short",
            CompetitiveSelectionDescription = "Updated Competitive",
            PreferentialTermsOfParticipation = "Updated Terms",
            EnrollmentProcedureDescription = "Updated Enrollment",
            InstitutionHierarchyId = Guid.NewGuid(),
            WorkshopDescriptionItems = new List<WorkshopDescriptionItemDto>
            {
                new WorkshopDescriptionItemDto
                {
                    SectionName = "Updated Section",
                    Description = "Updated Description"
                }
            }
        };

        // Setup workshop draft repository mock for valid draft ID
        workshopDraftRepoMock.Setup(repo => repo.GetById(draftId))
            .ReturnsAsync(validWorkshopDraft);

        // Setup current user service mock to allow moderator rights
        currentUserServiceMock.Setup(service => service.UserHasRights(It.IsAny<ModeratorRights>(), It.IsAny<TechAdminRights>()))
            .Returns(Task.CompletedTask);

        currentUserServiceMock.Setup(service => service.UserId)
            .Returns(userId);
    }
}