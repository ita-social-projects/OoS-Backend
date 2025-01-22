using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class DirectionServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private TestOutOfSchoolDbContext context;
    private IEntityRepositorySoftDeleted<long, Direction> repo;
    private IWorkshopRepository repositoryWorkshop;
    private DirectionService service;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<ILogger<DirectionService>> logger;
    private Mock<IMapper> mapper;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;

    [SetUp]
    public void SetUp()
    {
        mapper = new Mock<IMapper>();
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);

        repo = new EntityRepositorySoftDeleted<long, Direction>(context);
        repositoryWorkshop = new WorkshopRepository(context);
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        logger = new Mock<ILogger<DirectionService>>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();

        service = new DirectionService(
            repo,
            repositoryWorkshop,
            logger.Object,
            localizer.Object,
            mapper.Object,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            regionAdminServiceMock.Object);

        SeedDatabase();
    }

    [Test]
    [Order(1)]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var expected = new Direction()
        {
            Title = "NewTitle",
            Description = "NewDescription",
        };

        var input = new DirectionDto()
        {
            Title = "NewTitle",
            Description = "NewDescription",
        };

        mapper.Setup(m => m.Map<Direction>(input)).Returns(expected);
        mapper.Setup(m => m.Map<DirectionDto>(expected)).Returns(input);

        // Act
        var result = await service.Create(input).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Title, result.Title);
        Assert.AreEqual(expected.Description, result.Description);
    }

    [Test]
    [Order(2)]
    public async Task Create_NotUniqueEntity_ReturnsArgumentException()
    {
        // TODO: Make independent test
        // Arrange
        var expected = (await repo.GetAll()).FirstOrDefault();
        var input = new DirectionDto()
        {
            Title = expected.Title,
            Description = expected.Description,
        };

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await service.Create(input).ConfigureAwait(false));
    }

    [Test]
    [Order(3)]
    public async Task GetAll_WhenCalled_ReturnsAllEntities()
    {
        // Arrange
        var expected = await repo.GetAll();

        // Act
        var result = await service.GetAll().ConfigureAwait(false);

        // Assert
        Assert.That(expected.Count(), Is.EqualTo(result.Count()));
    }

    [Test]
    [Order(4)]
    [TestCase(1)]
    public async Task GetById_WhenIdIsValid_ReturnsEntity(long id)
    {
        // Arrange
        var expected = await repo.GetById(id);

        var expectedDto = new DirectionDto()
        {
            Id = expected.Id,
            Title = expected.Title,
        };

        mapper.Setup(m => m.Map<DirectionDto>(expected)).Returns(expectedDto);

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Id, result.Id);
    }

    [Test]
    [Order(5)]
    [TestCase(10)]
    public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
    {
        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.GetById(id).ConfigureAwait(false));
    }

    [Test]
    [Order(8)]
    [TestCase(1)]
    public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
    {
        // Act
        var countBeforeDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

        await ((ISensitiveDirectionService)service).Delete(id);

        var countAfterDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

        // Assert
        Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
    }

    [Test]
    [Order(9)]
    [TestCase(10)]
    public async Task Delete_WhenIdIsInvalid_DirectionNotExists(long id)
    {
        // Act
        var result = await service.Delete(id).ConfigureAwait(false);

        // Assert
        Assert.False(result.Succeeded);
        Assert.AreEqual(result.OperationResult.Errors.ElementAt(0).Description, $"Direction with Id = {id} is not exists.");
    }

    [Test]
    [Order(10)]
    [TestCase(2)]
    public async Task Delete_WhenThereAreRelatedWorkshops_ReturnsNotSucceeded(long id)
    {
        // Act
        var result = await service.Delete(id).ConfigureAwait(false);

        // Assert
        Assert.False(result.Succeeded);
        Assert.That(result.OperationResult.Errors, Is.Not.Empty);
    }

    [Test]
    [Order(11)]
    public async Task GetByFilter_WhenMinistryAdminCalled_ReturnDirections()
    {
        // Arrange
        var filter = new DirectionFilter();
        var institutionId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771");

        var expected = await repo.GetByFilter(
            d => d.InstitutionHierarchies.Any(
                i => i.InstitutionId == institutionId),
            includeProperties: "InstitutionHierarchies");

        var expectedDto = new DirectionDto()
        {
            Id = expected.FirstOrDefault().Id,
            Title = expected.FirstOrDefault().Title,
            Description = expected.FirstOrDefault().Description,
        };

        mapper.Setup(m => m.Map<DirectionDto>(It.IsAny<Direction>())).Returns(expectedDto);

        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(true);
        ministryAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<MinistryAdminDto>(new MinistryAdminDto()
            {
                InstitutionId = institutionId,
            }));

        // Act
        var result = await service.GetByFilter(filter, true).ConfigureAwait(false);

        // Assert
        Assert.True(result.Entities.All(d => d.Title == expectedDto.Title));
    }

    [Test]
    [Order(12)]
    public async Task GetByFilter_WhenRegionAdminCalled_ReturnDirections()
    {
        // Arrange
        var filter = new DirectionFilter();
        var institutionId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771");

        var expected = await repo.GetByFilter(
            d => d.InstitutionHierarchies.Any(
                i => i.InstitutionId == institutionId),
            includeProperties: "InstitutionHierarchies");

        var expectedDto = new DirectionDto()
        {
            Id = expected.FirstOrDefault().Id,
            Title = expected.FirstOrDefault().Title,
            Description = expected.FirstOrDefault().Description,
        };

        mapper.Setup(m => m.Map<DirectionDto>(It.IsAny<Direction>())).Returns(expectedDto);

        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(true);
        regionAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult<RegionAdminDto>(new RegionAdminDto()
            {
                InstitutionId = institutionId,
            }));

        // Act
        var result = await service.GetByFilter(filter, true).ConfigureAwait(false);

        // Assert
        Assert.True(result.Entities.All(d => d.Title == expectedDto.Title));
    }

    private void SeedDatabase()
    {
        using var ctx = new TestOutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            ctx.Institutions.Add(new Institution()
            {
                Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                NumberOfHierarchyLevels = 1,
                Title = "Title"
            });

            var directions = new List<Direction>()
            {
                new Direction()
                {
                    Title = "Test1",
                    Description = "Test1",
                },
                new Direction
                {
                    Title = "Test2",
                    Description = "Test2",
                    InstitutionHierarchies = new List<InstitutionHierarchy>()
                    {
                        new InstitutionHierarchy()
                        {
                            Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                            Title = "Title",
                            HierarchyLevel = 1,
                            InstitutionId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                        },
                    },
                },
                new Direction
                {
                    Title = "Test3",
                    Description = "Test3",
                },
            };

            ctx.Directions.AddRange(directions);

            var workshops = new List<Workshop>()
            {
                new Workshop()
                {
                    Title = "Test1",
                    InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                },
            };

            ctx.Workshops.AddRange(workshops);

            ctx.SaveChanges();
        }
    }
}