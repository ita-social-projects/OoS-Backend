using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Redis;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.SubordinationStructure;
using OutOfSchool.WebApi.Services.SubordinationStructure;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class InstitutionHierarchyServiceTests
{
    private Mock<IInstitutionHierarchyRepository> repo;
    private Mock<IWorkshopRepository> repositoryWorkshop;
    private Mock<IProviderRepository> repositoryProvider;
    private IInstitutionHierarchyService service;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<ILogger<InstitutionHierarchyService>> logger;
    private Mock<IMapper> mapper;
    private Mock<ICacheService> cache;

    [SetUp]
    public void SetUp()
    {
        repo = new Mock<IInstitutionHierarchyRepository>();
        repositoryWorkshop = new Mock<IWorkshopRepository>();
        repositoryProvider = new Mock<IProviderRepository>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        logger = new Mock<ILogger<InstitutionHierarchyService>>();
        mapper = new Mock<IMapper>();
        cache = new Mock<ICacheService>();
        service = new InstitutionHierarchyService(
            repo.Object,
            repositoryWorkshop.Object,
            repositoryProvider.Object,
            logger.Object,
            localizer.Object,
            mapper.Object,
            cache.Object);
    }

    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        Guid institutionId = Guid.NewGuid();

        var expected = new InstitutionHierarchy()
        {
            Title = "NewTitle",
            HierarchyLevel = 1,
            InstitutionId = institutionId,
        };

        var input = new InstitutionHierarchyDto()
        {
            Title = "NewTitle",
            HierarchyLevel = 1,
            InstitutionId = institutionId,
        };

        mapper.Setup(m => m.Map<InstitutionHierarchy>(It.IsAny<InstitutionHierarchyDto>())).Returns(expected);
        mapper.Setup(m => m.Map<InstitutionHierarchyDto>(It.IsAny<InstitutionHierarchy>())).Returns(input);
        repo.Setup(r => r.Create(expected)).ReturnsAsync(expected);

        // Act
        var result = await service.Create(input).ConfigureAwait(false);

        // Assert
        repo.Verify(r => r.Create(expected), Times.Once);
        Assert.AreEqual(expected.Title, result.Title);
        Assert.AreEqual(expected.HierarchyLevel, result.HierarchyLevel);
        Assert.AreEqual(expected.InstitutionId, result.InstitutionId);
    }

    [Test]
    public async Task Create_NotUniqueEntity_ReturnsNull()
    {
        // Arrange
        Guid institutionId = Guid.NewGuid();

        var input = new InstitutionHierarchyDto()
        {
            Title = "NewTitle",
            HierarchyLevel = 1,
            InstitutionId = institutionId,
        };
        var mockDbResponse = new List<InstitutionHierarchy>()
        {
            new InstitutionHierarchy()
            {
                Title = "NewTitle",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
            },
        }.AsTestAsyncEnumerableQuery();

        repo.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(),
                null,
                false))
            .Returns(mockDbResponse);

        // Act
        var expected = await service.Create(input).ConfigureAwait(false);

        // Assert
        Assert.IsNull(expected);
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsAllEntities()
    {
        // Arrange
        Guid institutionId = Guid.NewGuid();

        var expectedEntity = new List<InstitutionHierarchy>()
        {
            new InstitutionHierarchy()
            {
                Title = "NewTitle1",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
            },
            new InstitutionHierarchy()
            {
                Title = "NewTitle2",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
            },
            new InstitutionHierarchy()
            {
                Title = "NewTitle3",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
            },
        };

        var expectedDto = new List<InstitutionHierarchyDto>()
        {
            new InstitutionHierarchyDto()
            {
                Title = "NewTitle1",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
            },
            new InstitutionHierarchyDto()
            {
                Title = "NewTitle2",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
            },
            new InstitutionHierarchyDto()
            {
                Title = "NewTitle3",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
            },
        };

        repo.Setup(r => r.GetAll()).ReturnsAsync(expectedEntity);
        mapper.Setup(m => m.Map<List<InstitutionHierarchyDto>>(expectedEntity)).Returns(expectedDto);

        // Act
        var result = await service.GetAll().ConfigureAwait(false);

        // Assert
        repo.Verify(r => r.GetAll(), Times.Once);
        Assert.That(expectedEntity.Count(), Is.EqualTo(result.Count()));
    }

    [Test]
    [TestCase("9546482E-887A-4CAB-A403-AD9C326FFDA5")]
    public async Task GetById_WhenIdIsValid_ReturnsEntity(string idStr)
    {
        // Arrange
        Guid id = Guid.Parse(idStr);

        var expected = new InstitutionHierarchyDto()
        {
            Title = "NewTitle1",
            HierarchyLevel = 1,
            InstitutionId = id,
        };

        var mockDbEntry = new InstitutionHierarchy()
        {
            Title = "NewTitle1",
            HierarchyLevel = 1,
            InstitutionId = id,
        };

        repo.Setup(r => r.GetById(id)).ReturnsAsync(mockDbEntry);
        mapper.Setup(m => m.Map<InstitutionHierarchyDto>(mockDbEntry)).Returns(expected);

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCase("9546482E-887A-4CAB-A403-AD9C326FFDA5")]
    public async Task GetById_WhenIdIsInvalid_ReturnNullIfOutOfRange(string idStr)
    {
        // Act
        var expected = await service.GetById(Guid.Parse(idStr)).ConfigureAwait(false);

        // Assert
        Assert.IsNull(expected);
    }

    [Test]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        Guid institutionId = Guid.NewGuid();
        Guid hierarchyId = Guid.NewGuid();
        List<long> directionsIds = new List<long> { 1 };
        List<Direction> directions = new List<Direction> { new Direction() { Id = 1, Title = "Direction" } };
        List<DirectionDto> directionsDtos = new List<DirectionDto> { new DirectionDto() { Id = 1, Title = "Direction" } };

        var changedEntity = new InstitutionHierarchy()
        {
            Id = hierarchyId,
            Title = "ChangedTitle1",
            HierarchyLevel = 1,
            InstitutionId = institutionId,
            Directions = directions,
        };

        var changedDto = new InstitutionHierarchyDto()
        {
            Id = hierarchyId,
            Title = "ChangedTitle1",
            HierarchyLevel = 1,
            InstitutionId = institutionId,
            Directions = directionsDtos,
        };

        mapper.Setup(m => m.Map<InstitutionHierarchyDto>(changedEntity)).Returns(changedDto);
        mapper.Setup(m => m.Map<InstitutionHierarchy>(changedDto)).Returns(changedEntity);
        repo.Setup(r => r.Update(changedEntity, directionsIds)).ReturnsAsync(changedEntity);

        // Act
        var result = await service.Update(changedDto).ConfigureAwait(false);

        // Assert
        repo.Verify(r => r.Update(changedEntity, directionsIds), Times.Once);
        Assert.That(changedEntity.Title, Is.EqualTo(result.Title));
    }

    [Test]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        Guid institutionId = Guid.NewGuid();
        Guid hierarchyId = Guid.NewGuid();
        List<long> directionsIds = new List<long> { 1 };
        List<Direction> directions = new List<Direction> { new Direction() { Id = 1, Title = "Direction" } };
        List<DirectionDto> directionsDtos = new List<DirectionDto> { new DirectionDto() { Id = 1, Title = "Direction" } };

        var changedEntity = new InstitutionHierarchy()
        {
            Id = hierarchyId,
            Title = "ChangedTitle1",
            HierarchyLevel = 1,
            InstitutionId = institutionId,
            Directions = directions,
        };

        var changedDto = new InstitutionHierarchyDto()
        {
            Id = hierarchyId,
            Title = "ChangedTitle1",
            HierarchyLevel = 1,
            InstitutionId = institutionId,
            Directions = directionsDtos,
        };

        mapper.Setup(m => m.Map<InstitutionHierarchy>(changedDto)).Returns(changedEntity);
        repo.Setup(r => r.Update(changedEntity, directionsIds)).Throws<DbUpdateConcurrencyException>();

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedDto).ConfigureAwait(false));
    }

    [Test]
    [TestCase("9546482E-887A-4CAB-A403-AD9C326FFDA5")]
    public async Task Delete_WhenIdIsValid_DeletesEntity(string idStr)
    {
        // Arrange
        Guid id = Guid.Parse(idStr);

        var deleted = new InstitutionHierarchyDto()
        {
            Id = id,
        };

        mapper.Setup(m => m.Map<InstitutionHierarchyDto>(It.IsAny<InstitutionHierarchy>())).Returns(deleted);

        // Act
        var result = await service.Delete(id).ConfigureAwait(false);

        // Assert
        repo.Verify(r => r.Delete(It.IsAny<InstitutionHierarchy>()), Times.Once);
        Assert.True(result.Succeeded);
    }

    [Test]
    [TestCase("9546482E-887A-4CAB-A403-AD9C326FFDA5")]
    public void Delete_WhenIdIsInvalid_ThrowsDbUpdateConcurrencyException(string idStr)
    {
        // Arrange
        Guid id = Guid.Parse(idStr);

        repo.Setup(r => r.Delete(It.IsAny<InstitutionHierarchy>())).Throws<DbUpdateConcurrencyException>();

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Delete(id).ConfigureAwait(false));
    }

    [Test]
    [TestCase("9546482E-887A-4CAB-A403-AD9C326FFDA5")]
    public async Task GetChildren_WhenIdIsValid_ReturnsEntities(string idStr)
    {
        // Arrange
        Guid parentId = Guid.Parse(idStr);
        Guid institutionId = Guid.NewGuid();

        var expectedEntity = new List<InstitutionHierarchy>()
        {
            new InstitutionHierarchy()
            {
                Title = "NewTitle1",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
                ParentId = parentId,
            },
            new InstitutionHierarchy()
            {
                Title = "NewTitle2",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
                ParentId = parentId,
            },
            new InstitutionHierarchy()
            {
                Title = "NewTitle3",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
                ParentId = parentId,
            },
        };

        var expectedDto = new List<InstitutionHierarchyDto>()
        {
            new InstitutionHierarchyDto()
            {
                Title = "NewTitle1",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
                ParentId = parentId,
            },
            new InstitutionHierarchyDto()
            {
                Title = "NewTitle2",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
                ParentId = parentId,
            },
            new InstitutionHierarchyDto()
            {
                Title = "NewTitle3",
                HierarchyLevel = 1,
                InstitutionId = institutionId,
                ParentId = parentId,
            },
        };

        repo.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<InstitutionHierarchy, bool>>>(), string.Empty))
            .ReturnsAsync(expectedEntity);

        mapper.Setup(m => m.Map<List<InstitutionHierarchyDto>>(It.IsAny<List<InstitutionHierarchy>>())).Returns(expectedDto);

        // Act
        var entities = await service.GetChildrenFromDatabase(parentId).ConfigureAwait(false);

        // Assert
        Assert.That(expectedDto.Count(), Is.EqualTo(entities.Count()));
    }

    [Test]
    [TestCase("9546482E-887A-4CAB-A403-AD9C326FFDA5")]
    public async Task GetChildren_WhenIdIsInvalid_ReturnNull(string idStr)
    {
        // Act
        var expected = await service.GetChildrenFromDatabase(Guid.Parse(idStr)).ConfigureAwait(false);

        // Assert
        Assert.IsEmpty(expected);
    }
}