using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.SportsRegistry;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class SportSectionSyncServiceTests
{
    private Mock<ISportsRegistryWorkshopProvider> sectionsProviderMock;
    private Mock<IInstitutionHierarchyRepository> hierarchyRepositoryMock;
    private Mock<IWorkshopDraftRepository> workshopDraftRepositoryMock;
    private Mock<IWorkshopRepository> workshopRepositoryMock;
    private Mock<ICodeficatorRepository> codeficatorRepositoryMock;
    private Mock<IProviderRepository> providerRepositoryMock;
    private Mock<ILogger<SportsSectionSyncService>> loggerMock;

    private Guid ministryId;
    long sportKindIdCode;
    string organizationCode;
    Guid sectionId;

    [SetUp]
    public void SetUp()
    {
        sectionsProviderMock = new Mock<ISportsRegistryWorkshopProvider>();
        workshopDraftRepositoryMock = new Mock<IWorkshopDraftRepository>();
        workshopRepositoryMock = new Mock<IWorkshopRepository>();
        codeficatorRepositoryMock = new Mock<ICodeficatorRepository>();
        providerRepositoryMock = new Mock<IProviderRepository>();
        hierarchyRepositoryMock = new Mock<IInstitutionHierarchyRepository>();

        loggerMock = new Mock<ILogger<SportsSectionSyncService>>();
        ministryId = Guid.NewGuid();
        sportKindIdCode = 55;
        organizationCode = "45080641";
        sectionId = Guid.NewGuid();
    }

    [Test]
    public async Task SyncSportsSectionsAsync_WhenEmptyList_ShouldReturnZero()
    {
        SetupEmptySectionsProvider();

        var service = CreateService();

        var result = await service.SyncSportsSectionsAsync();

        result.Should().Be(0);
        workshopDraftRepositoryMock.Verify(r => r.RunInTransaction(It.IsAny<Func<Task>>()), Times.Never);
    }

    [Test]
    public async Task SyncSportsSectionsAsync_WhenApiReturnsError_ShouldFallbackToFullFetch_AndReturnZero()
    {
        // Arrange
        var error = new ErrorResponse { Message = "API failure" };
        var failedResponse = (Either<ErrorResponse, List<ExternalSportsSectionDto>>)(error);
        var successResponse = (Either<ErrorResponse, List<ExternalSportsSectionDto>>)(new List<ExternalSportsSectionDto>());

        sectionsProviderMock
            .SetupSequence(p => p.GetAllSportsSectionsAsync(
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int>()))
            .ReturnsAsync(failedResponse)  // first call with error
            .ReturnsAsync(successResponse); // second call (fallback)

        var service = CreateService();

        // Act
        var result = await service.SyncSportsSectionsAsync();

        // Assert
        result.Should().Be(0);

        sectionsProviderMock.Verify(p =>
            p.GetAllSportsSectionsAsync(
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int>()),
            Times.Exactly(2));

        workshopDraftRepositoryMock.Verify(r => r.RunInTransaction(It.IsAny<Func<Task>>()), Times.Never);

        loggerMock.Verify(
           x => x.Log(
           It.Is<LogLevel>(l => l == LogLevel.Warning),
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("fallback to full fetch")),
           It.IsAny<Exception>(),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
       Times.Once);
    }

    [Test]
    public async Task SyncSportsSectionsAsync_WhenNewSection_ShouldCreateDraft()
    {
        // Arrange
        var section = BuildSection(sectionId, sportKindIdCode, organizationCode);

        SetupSectionsProviderWithSections(new List<ExternalSportsSectionDto> { section });
        providerRepositoryMock.Setup(p => p.GetIdByEdrpouAsync(organizationCode))
            .ReturnsAsync(Guid.NewGuid());

        SetUpEmptyWorkshops();
        SetUpEmptyDrafts();
       
        var sportHierarchy = BuildHierarchy();
        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy>() { sportHierarchy });

        codeficatorRepositoryMock
            .Setup(r => r.GetIdByCodeAsync(
                It.IsAny<string>()))
            .ReturnsAsync(sportKindIdCode);

        IEnumerable<WorkshopDraft> createdDrafts = null!;

        workshopDraftRepositoryMock
            .Setup(r => r.Create(It.IsAny<IEnumerable<WorkshopDraft>>()))
            .Callback<IEnumerable<WorkshopDraft>>(drafts => createdDrafts = drafts.ToList())
            .Returns<IEnumerable<WorkshopDraft>>(drafts => Task.FromResult(drafts));

        workshopDraftRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(func => func());

        var service = CreateService();

        // Act
        var result = await service.SyncSportsSectionsAsync();

        // Assert
        result.Should().Be(1);
        createdDrafts.Should().NotBeNull();
        createdDrafts.Should().ContainSingle();
        createdDrafts.First().WorkshopDraftContent.Title.Should().Be("New Sports Section");

        workshopDraftRepositoryMock.Verify(r => r.Create(It.IsAny<IEnumerable<WorkshopDraft>>()), Times.Once);
        workshopDraftRepositoryMock.Verify(r => r.RunInTransaction(It.IsAny<Func<Task>>()), Times.Once);
    }

    [Test]
    public async Task SyncSportsSectionsAsync_WhenExistingDraft_ShouldUpdateDraft()
    {
        // Arrange
        var section = new ExternalSportsSectionDto
        {
            SectionId = sectionId,
            SectionName = "Updated Sports Section",
            SectionSportKindDictIdCode = sportKindIdCode,
            OrganizationCode = organizationCode,
            UpdatedInRegistryAt = DateTime.UtcNow
        };

        SetupSectionsProviderWithSections(new List<ExternalSportsSectionDto> { section });
        providerRepositoryMock
            .Setup(p => p.GetIdByEdrpouAsync(organizationCode))
            .ReturnsAsync(Guid.NewGuid());

        var existingDraft = new WorkshopDraft
        {
            Id = Guid.NewGuid(),
            MinsportSectionId = sectionId,
            DraftStatus = WorkshopDraftStatus.Draft,
            WorkshopDraftContent = new WorkshopDraftContent()
        };

        SetUpDraftsWithData(new List<WorkshopDraft> { existingDraft });
        SetUpEmptyWorkshops();

        var sportHierarchy = BuildHierarchy(sportKindIdCode);
        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy> { sportHierarchy });

        codeficatorRepositoryMock
            .Setup(r => r.GetIdByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(12345);

        WorkshopDraft? updatedDraft = null;

        workshopDraftRepositoryMock
            .Setup(r => r.Update(It.IsAny<WorkshopDraft>()))
            .Callback<WorkshopDraft>(d => updatedDraft = d)
            .Returns<WorkshopDraft>(d => Task.FromResult(d));

        workshopDraftRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportsSectionsAsync();

        // Assert
        result.Should().Be(1);
        updatedDraft.Should().NotBeNull();
        updatedDraft!.DraftStatus.Should().Be(WorkshopDraftStatus.PendingModeration);
        updatedDraft.WorkshopDraftContent.InstitutionHierarchyId.Should().Be(sportHierarchy.Id);

        workshopDraftRepositoryMock.Verify(r => r.Update(It.IsAny<WorkshopDraft>()), Times.Once);
        workshopDraftRepositoryMock.Verify(r => r.Create(It.IsAny<IEnumerable<WorkshopDraft>>()), Times.Never);
    }

    [Test]
    public async Task SyncSportsSectionsAsync_WhenWorkshopExists_ShouldCreateDraftFromWorkshop()
    {
        // Arrange
        var section = new ExternalSportsSectionDto
        {
            SectionId = sectionId,
            SectionName = "Section With Workshop",
            SectionSportKindDictIdCode = sportKindIdCode,
            OrganizationCode = organizationCode,
            UpdatedInRegistryAt = DateTime.UtcNow
        };

        SetupSectionsProviderWithSections(new List<ExternalSportsSectionDto> { section });

        var existingWorkshop = new Workshop
        {
            Id = Guid.NewGuid(),
            MinsportSectionId = sectionId,
            ProviderId = Guid.NewGuid()
        };

        SetUpWorkshopsWithData(new List<Workshop> { existingWorkshop });
        SetUpEmptyDrafts();

        var sportHierarchy = BuildHierarchy(sportKindIdCode);
        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy> { sportHierarchy });

        codeficatorRepositoryMock
            .Setup(r => r.GetIdByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(12345);

        IEnumerable<WorkshopDraft>? createdDrafts = null;

        workshopDraftRepositoryMock
            .Setup(r => r.Create(It.IsAny<IEnumerable<WorkshopDraft>>()))
            .Callback<IEnumerable<WorkshopDraft>>(drafts => createdDrafts = drafts.ToList())
            .Returns<IEnumerable<WorkshopDraft>>(drafts => Task.FromResult(drafts));

        workshopDraftRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportsSectionsAsync();

        // Assert
        result.Should().Be(1);
        createdDrafts.Should().NotBeNull();
        createdDrafts.First().WorkshopId.Should().Be(existingWorkshop.Id);
        createdDrafts.First().MinsportSectionId.Should().Be(existingWorkshop.MinsportSectionId);
        createdDrafts.First().DraftStatus.Should().Be(WorkshopDraftStatus.PendingModeration);

        workshopDraftRepositoryMock.Verify(r => r.Create(It.IsAny<IEnumerable<WorkshopDraft>>()), Times.Once);
        workshopDraftRepositoryMock.Verify(r => r.Update(It.IsAny<WorkshopDraft>()), Times.Never);
    }

    [Test]
    public async Task SyncSportsSectionsAsync_WhenProviderNotFound_ShouldSkipSection()
    {
        // Arrange
        var section = BuildSection(sectionId, sportKindIdCode);
        
        SetupSectionsProviderWithSections(new List<ExternalSportsSectionDto> { section });

        var sportHierarchy = BuildHierarchy(sportKindIdCode);
        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy> { sportHierarchy });

        providerRepositoryMock
            .Setup(p => p.GetIdByEdrpouAsync(It.IsAny<string>()))
            .ReturnsAsync((Guid?)null);

        codeficatorRepositoryMock
            .Setup(c => c.GetIdByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(1234L);

        var createdDrafts = new List<WorkshopDraft>();
        workshopDraftRepositoryMock
            .Setup(r => r.Create(It.IsAny<IEnumerable<WorkshopDraft>>()))
            .Callback<IEnumerable<WorkshopDraft>>(d => createdDrafts.AddRange(d))
            .ReturnsAsync((IEnumerable<WorkshopDraft> d) => d);

        var service = CreateService();

        // Act
        var result = await service.SyncSportsSectionsAsync();

        // Assert
        result.Should().Be(0);
        createdDrafts.Should().BeEmpty();
        
        VerifyLog(LogLevel.Warning, "provider was not found", Times.Once());
    }

    [Test]
    public async Task SyncSportsSectionsAsync_WhenHierarchyNotFound_ShouldSkipSection()
    {
        // Arrange
        var section = new ExternalSportsSectionDto
        {
            SectionId = sectionId,
            SectionName = "Section Without Hierarchy",
            SectionSportKindDictIdCode = sportKindIdCode,
            OrganizationCode = "12345678",
        };

        SetupSectionsProviderWithSections(new List<ExternalSportsSectionDto> { section });

        providerRepositoryMock
            .Setup(p => p.GetIdByEdrpouAsync(It.IsAny<string>()))
            .ReturnsAsync(Guid.NewGuid());

        codeficatorRepositoryMock
            .Setup(c => c.GetIdByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(1234L);

        // hierarchy not found
        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy>());

        SetUpEmptyWorkshops();
        SetUpEmptyDrafts();

        var createdDrafts = new List<WorkshopDraft>();
        workshopDraftRepositoryMock
            .Setup(r => r.Create(It.IsAny<IEnumerable<WorkshopDraft>>()))
            .Callback<IEnumerable<WorkshopDraft>>(d => createdDrafts.AddRange(d))
            .ReturnsAsync((IEnumerable<WorkshopDraft> d) => d);

        var service = CreateService();

        // Act
        var result = await service.SyncSportsSectionsAsync();

        // Assert
        result.Should().Be(0);
        createdDrafts.Should().BeEmpty();

        VerifyLog(LogLevel.Warning, "no InstitutionHierarchy found", Times.Once());
    }
    [Test]
    public async Task SyncSportsSectionsAsync_WhenRunInTransactionThrows_ShouldLogErrorAndReturnZero()
    {
        // Arrange
        var section = new ExternalSportsSectionDto
        {
            SectionId = sectionId,
            SectionName = "Section With Transaction Error",
            SectionSportKindDictIdCode = sportKindIdCode,
            OrganizationCode = organizationCode,
        };

        SetupSectionsProviderWithSections(new List<ExternalSportsSectionDto> { section });

        providerRepositoryMock
            .Setup(p => p.GetIdByEdrpouAsync(It.IsAny<string>()))
            .ReturnsAsync(Guid.NewGuid());

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy> { BuildHierarchy(sportKindIdCode) });

        codeficatorRepositoryMock
            .Setup(c => c.GetIdByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(1234L);

        SetUpEmptyWorkshops();
        SetUpEmptyDrafts();
       
        workshopDraftRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .ThrowsAsync(new Exception("DB transaction failed"));

        var service = CreateService();

        // Act
        var result = await service.SyncSportsSectionsAsync();

        // Assert
        result.Should().Be(0);

        VerifyLog(LogLevel.Error, "Error during sports sections sync", Times.Once()); 
    }

    private SportsSectionSyncService CreateService()
    {
        return new SportsSectionSyncService(
            sectionsProviderMock.Object,
            workshopDraftRepositoryMock.Object,
            workshopRepositoryMock.Object,
            codeficatorRepositoryMock.Object,
            providerRepositoryMock.Object,
            hierarchyRepositoryMock.Object,
            loggerMock.Object);
    }
    private ExternalSportsSectionDto BuildSection(Guid id, long sportKindIdCode = 55, string organizationCode = "45080641")
    {
        var section = new ExternalSportsSectionDto
        {
            SectionId = id,
            SectionName = "New Sports Section",
            SectionSportKindDictIdCode = sportKindIdCode,
            OrganizationCode = organizationCode,
        };
        return section;
    }
    private InstitutionHierarchy BuildHierarchy(long sportKindId = 55)
    {
        return new InstitutionHierarchy
        {
            Id = Guid.NewGuid(),
            Title = "Hierarchy Title",
            InstitutionId = ministryId,
            SportRegistryIdCode = sportKindId,
            HierarchyLevel = 2,
            SportsSectionNumeral = "football",
            RegistrySyncDate = DateTime.UtcNow
        };
    }

    private void SetUpEmptyDrafts() =>
    workshopDraftRepositoryMock
        .Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
        .ReturnsAsync(new List<WorkshopDraft>());

    private void SetUpDraftsWithData(IEnumerable<WorkshopDraft> drafts) =>
        workshopDraftRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<WorkshopDraft, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<WorkshopDraft>, IQueryable<WorkshopDraft>>>()))
            .ReturnsAsync(drafts);
    private void SetUpEmptyWorkshops() =>
    workshopRepositoryMock
        .Setup(r => r.GetByFilter(
            It.IsAny<Expression<Func<Workshop, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Func<IQueryable<Workshop>, IQueryable<Workshop>>>()))
        .ReturnsAsync(new List<Workshop>());

    private void SetUpWorkshopsWithData(IEnumerable<Workshop> workshops) =>
        workshopRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Workshop>, IQueryable<Workshop>>>()))
            .ReturnsAsync(workshops);

    private void SetupSectionsProviderWithSections(IEnumerable<ExternalSportsSectionDto> sections)
    {
        var apiResponse = (Either<ErrorResponse, List<ExternalSportsSectionDto>>)sections.ToList();
        sectionsProviderMock
            .Setup(p => p.GetAllSportsSectionsAsync(
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int>()))
            .ReturnsAsync(apiResponse);
    }

    private void SetupEmptySectionsProvider()
    {
        SetupSectionsProviderWithSections(new List<ExternalSportsSectionDto>());
    }

    private void VerifyLog(LogLevel level, string containsText, Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(containsText)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
    }
}
