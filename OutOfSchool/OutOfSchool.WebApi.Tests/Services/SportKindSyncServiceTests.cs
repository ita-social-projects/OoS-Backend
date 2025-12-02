using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.SportsRegistry;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class SportKindSyncServiceTests
{
    private Mock<ISportsRegistryDictionaryProvider> dictionaryProviderMock;
    private Mock<IInstitutionHierarchyRepository> hierarchyRepositoryMock;
    private Mock<ILogger<SportKindSyncService>> loggerMock;
    private IOptions<InstitutionOptions> institutionOptions;
    private Guid ministryId;

    [SetUp]
    public void SetUp()
    {
        dictionaryProviderMock = new Mock<ISportsRegistryDictionaryProvider>();
        hierarchyRepositoryMock = new Mock<IInstitutionHierarchyRepository>();
        loggerMock = new Mock<ILogger<SportKindSyncService>>();
        ministryId = Guid.NewGuid();
        institutionOptions = Options.Create(new InstitutionOptions { MinistryOfSportId = ministryId.ToString() });
    }

    private SportKindSyncService CreateService()
    {
        return new SportKindSyncService(
            dictionaryProviderMock.Object,
            hierarchyRepositoryMock.Object,
            loggerMock.Object,
            institutionOptions);
    }

    private SportKindDto BuildSportKindDto(long code, string name = "Football", DateTime? updatedAt = null)
    {
        return new SportKindDto
        {
            Id = Guid.NewGuid(),
            IdCode = code,
            Name = name,
            SportKindSectionNumeral = "01",
            SportKindSectionIdCode = 1,
            IsActive = true,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }

    private InstitutionHierarchy BuildEntity(long code, DateTime? syncDate = null)
    {
        return new InstitutionHierarchy
        {
            Id = Guid.NewGuid(),
            Title = "Old title",
            InstitutionId = ministryId,
            HierarchyLevel = 2,
            SportRegistryIdCode = code,
            SportsSectionNumeral = "XX",
            RegistrySyncDate = syncDate
        };
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenApiReturnsError_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = new ErrorResponse { Message = "API failure" };
        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync((Either<ErrorResponse, List<SportKindDto>>)error);

        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.SyncSportKindsAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to fetch sport kinds: API failure");
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenApiReturnsEmptyList_ShouldReturnZero()
    {
        // Arrange
        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SportKindDto>());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(0);
        hierarchyRepositoryMock.Verify(r => r.RunInTransaction(It.IsAny<Func<Task>>()), Times.Never);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenAllNew_ShouldCreateEntities()
    {
        // Arrange
        var dtos = new List<SportKindDto>
        {
            BuildSportKindDto(1),
            BuildSportKindDto(2),
            BuildSportKindDto(3),
        };
        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(dtos);

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy>());

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(3);
        hierarchyRepositoryMock.Verify(r => r.Create(It.IsAny<IEnumerable<InstitutionHierarchy>>()), Times.Once);
        hierarchyRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<bool>(), default), Times.Never);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenAllNeedUpdate_ShouldUpdateEntities()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dtos = new List<SportKindDto>
        {
            BuildSportKindDto(10, "Football", now),
            BuildSportKindDto(20, "Basketball", now),
        };

        var entities = new List<InstitutionHierarchy>
        {
            BuildEntity(10, now.AddDays(-1)),
            BuildEntity(20, now.AddDays(-2)),
        };

        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(dtos);

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(entities);

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(2);
        hierarchyRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<bool>(), default), Times.Once);
        hierarchyRepositoryMock.Verify(r => r.Create(It.IsAny<IEnumerable<InstitutionHierarchy>>()), Times.Never);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenMixed_ShouldCreateAndUpdateEntities()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dtos = new List<SportKindDto>
        {
            BuildSportKindDto(100, "NewSport", now),
            BuildSportKindDto(200, "UpdatedSport", now),
        };

        var existing = new List<InstitutionHierarchy>
        {
            BuildEntity(200, now.AddDays(-1)), // needs update
        };

        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(dtos);

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(existing);

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(2);
        hierarchyRepositoryMock.Verify(r => r.Create(It.IsAny<IEnumerable<InstitutionHierarchy>>()), Times.Once);
        hierarchyRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<bool>(), default), Times.Once);
    }

    [Test]
    public async Task SyncSportKindsAsync_WhenNoChanges_ShouldReturnZeroAndNotCallTransaction()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dto = BuildSportKindDto(500, "Stable", now);
        var entity = BuildEntity(500, now); // UpdatedAt == RegistrySyncDate

        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SportKindDto> { dto });

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy> { entity });

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(0);
        hierarchyRepositoryMock.Verify(r => r.RunInTransaction(It.IsAny<Func<Task>>()), Times.Never);
    }
    
    [Test]
    public void SyncSportKindsAsync_WhenInvalidMinistryId_ShouldThrowFormatException()
    {
        // Arrange
        institutionOptions = Options.Create(new InstitutionOptions { MinistryOfSportId = "not-a-guid" });
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.SyncSportKindsAsync();

        // Assert
        act.Should().ThrowAsync<FormatException>();
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenRegistrySyncDateIsNull_ShouldUpdateEntity()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dto = BuildSportKindDto(600, "NeedsUpdate", now);
        var entity = BuildEntity(600, null); // RegistrySyncDate = null

        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SportKindDto> { dto });

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy> { entity });

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(1);
        hierarchyRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<bool>(), default), Times.Once);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenSingleNew_ShouldReturnOne()
    {
        // Arrange
        var dto = BuildSportKindDto(700);
        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SportKindDto> { dto });

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy>());

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(1);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenManySportKinds_ShouldProcessAll()
    {
        // Arrange
        var dtos = Enumerable.Range(1, 100)
            .Select(i => BuildSportKindDto(i))
            .ToList();

        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(dtos);

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy>());

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(100);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenSportKindNameIsEmpty_ShouldStillCreateEntity()
    {
        // Arrange
        var dto = BuildSportKindDto(800, "");
        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SportKindDto> { dto });

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy>());

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(1);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenSportKindIsInactive_ShouldStillBeProcessed()
    {
        // Arrange
        var dto = BuildSportKindDto(900, "Inactive");
        dto.IsActive = false;

        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SportKindDto> { dto });

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy>());

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(1);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenChangesExist_ShouldCallTransaction()
    {
        // Arrange
        var dto = BuildSportKindDto(1000);
        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<SportKindDto> { dto });

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy>());

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(1);
        hierarchyRepositoryMock.Verify(r => r.RunInTransaction(It.IsAny<Func<Task>>()), Times.Once);
    }
    
    [Test]
    public async Task SyncSportKindsAsync_WhenMixed_ShouldReturnSumOfChanges()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dtos = new List<SportKindDto>
        {
            BuildSportKindDto(2000, "CreateMe", now),
            BuildSportKindDto(3000, "UpdateMe", now),
        };

        var entity = BuildEntity(3000, now.AddDays(-1));

        dictionaryProviderMock
            .Setup(p => p.GetAllSportKindsAsync(It.IsAny<int>()))
            .ReturnsAsync(dtos);

        hierarchyRepositoryMock
            .Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>>>()))
            .ReturnsAsync(new List<InstitutionHierarchy> { entity });

        hierarchyRepositoryMock
            .Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        // Act
        var result = await service.SyncSportKindsAsync();

        // Assert
        result.Should().Be(2); // 1 created + 1 updated
    }
}