using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.BusinessLogic;
using OutOfSchool.Services.Models.SubordinationStructure;
using System.Collections.Generic;
using System;
using OutOfSchool.BusinessLogic.Models;
using System.Threading.Tasks;
using System.Linq;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class SubDirectionServiceTests
{
    private IEntityRepositorySoftDeleted<long, SubDirection> subDirectionRepository;
    private IEntityRepositorySoftDeleted<long, Direction> directionRepository;
    private ISubDirectionService service;
    private Mock<IMapper> mapper;
    private DbContextOptions<OutOfSchoolDbContext> options;

    [SetUp]
    public void SetUp()
    {
        mapper = new Mock<IMapper>();
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        var context = new TestOutOfSchoolDbContext(options);

        subDirectionRepository = new EntityRepositorySoftDeleted<long, SubDirection>(context);
        directionRepository = new EntityRepositorySoftDeleted<long, Direction>(context);
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        var logger = new Mock<ILogger<SubDirectionService>>();

        service = new SubDirectionService(
            subDirectionRepository,
            directionRepository,
            logger.Object,
            mapper.Object);

        SeedDatabase();
    }

    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var expected = new SubDirection()
        {
            Title = "NewTitle",
            Description = "NewDescription",
        };

        var input = new SubDirectionDto()
        {
            Title = "NewTitle",
            Description = "NewDescription",
        };

        var directionId = 1;

        mapper.Setup(m => m.Map<SubDirection>(input)).Returns(expected);
        mapper.Setup(m => m.Map<SubDirectionDto>(expected)).Returns(input);

        // Act
        var result = await service.Create(directionId, input).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Title, result.Value.Title);
        Assert.AreEqual(expected.Description, result.Value.Description);
    }

    [Test]
    public async Task Create_WhenEntityIsInvalid_ReturnsFailedResult()
    {
        // Arrange
        SubDirectionDto dto = null;
        var directionId = 1;

        // Act
        var result = await service.Create(directionId, dto).ConfigureAwait(false);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
    }

    [Test]
    public async Task Create_WhenDirectionIdIsInvalid_ReturnsFailedResult()
    {
        // Arrange
        var dto = new SubDirectionDto()
        {
            Title = "NewTitle",
            Description = "NewDescription",
        };
        var directionId = 10;

        // Act
        var result = await service.Create(directionId, dto).ConfigureAwait(false);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("404"));
    }

    [Test]
    public async Task Create_WhenSubDirectionWithSuchTitleAlreadyExists_ReturnsFailedResult()
    {
        // Arrange
        var dto = new SubDirectionDto()
        {
            Title = "Test1",
            Description = "Test1",
        };
        var directionId = 1;

        // Act
        var result = await service.Create(directionId, dto).ConfigureAwait(false);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
    }

    [Test]
    public async Task GetByFilter_WhenFilterIsValid_ReturnsEntities()
    {
        // Arrange
        var filter = new SearchStringFilter()
        {
            SearchString = "Test",
            From = 0,
            Size = 10,
        };

        // Act
        var result = await service.GetByFilter(1, filter).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, result.TotalAmount);
    }

    [Test]
    [TestCase(1)]
    public async Task GetById_WhenIdIsValid_ReturnsEntity(long id)
    {
        // Arrange
        var expected = await subDirectionRepository.GetById(id);

        var expectedDto = new SubDirectionDto()
        {
            Id = expected.Id,
            Title = expected.Title,
        };

        mapper.Setup(m => m.Map<SubDirectionDto>(expected)).Returns(expectedDto);

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Id, result.Id);
    }

    [Test]
    [TestCase(10)]
    public async Task GetById_WhenIdIsInvalid_ReturnsNull(long id)
    {
        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.IsNull(result);
    }

    private void SeedDatabase()
    {
        using var ctx = new TestOutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            var subDirections = new List<SubDirection>()
            {
                new SubDirection
                {
                    Id = 1,
                    Title = "Test1",
                    Description = "Test1",
                    Direction = new Direction
                    {
                        Title = "Test1",
                        Description = "Test1",
                    }
                },
                new SubDirection
                {
                    Id = 2,
                    Title = "Test2",
                    Description = "Test2",
                    Direction = new Direction
                    {
                        Title = "Test2",
                        Description = "Test2",
                    },
                    InstitutionHierarchies = new List<InstitutionHierarchy>()
                        {
                            new InstitutionHierarchy()
                            {
                                Id = Guid.NewGuid(),
                                Title = "Title",
                                HierarchyLevel = 1,
                                InstitutionId = Guid.NewGuid(),
                            },
                        },
                },
                new SubDirection
                {
                    Id = 3,
                    Title = "Test3",
                    Description = "Test3",
                },
            };

            ctx.SubDirections.AddRange(subDirections);
            ctx.SaveChanges();

        }
    }
}
