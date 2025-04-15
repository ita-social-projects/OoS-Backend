using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common.DbContextTests;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.BusinessLogic;
using System.Collections.Generic;
using OutOfSchool.Services.Models.SubordinationStructure;
using System;
using OutOfSchool.BusinessLogic.Models;
using System.Threading.Tasks;
using System.Linq;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class SubDirectionSensitiveServiceTests
{
    private IEntityRepositorySoftDeleted<long, SubDirection> subDirectionRepository;
    private IEntityRepositorySoftDeleted<long, Direction> directionRepository;
    private ISensitiveSubDirectionService service;
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
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var changedEntity = new SubDirectionDto()
        {
            Id = 1,
            Title = "ChangedTitle1",
        };

        var expected = await subDirectionRepository.GetById(changedEntity.Id).ConfigureAwait(false);

        mapper.Setup(m => m.Map<SubDirection>(changedEntity)).Returns(expected);
        mapper.Setup(m => m.Map<SubDirectionDto>(expected)).Returns(changedEntity);

        // Act
        var result = await service.Update(changedEntity).ConfigureAwait(false);

        // Assert
        Assert.That(changedEntity.Title, Is.EqualTo(result.Value.Title));
    }

    [Test]
    public async Task Update_WhenEntityIsInvalid_ReturnsFailedResult()
    {
        // Arrange
        SubDirectionDto dto = null;

        // Act
        var result = await service.Update(dto).ConfigureAwait(false);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("400"));
    }

    [Test]
    public async Task Update_WhenEntityDoesNotExist__ReturnsFailedResult()
    {
        // Arrange
        var dto = new SubDirectionDto()
        {
            Id = 123,
            Title = "ChangedTitle1",
        };

        // Act
        var result = await service.Update(dto).ConfigureAwait(false);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.OperationResult.Errors.FirstOrDefault().Code, Is.EqualTo("404"));
    }

    [Test]
    [Order(8)]
    [TestCase(1)]
    public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
    {
        // Act
        var result = await (service).Delete(id).ConfigureAwait(false);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    [TestCase(10)]
    public async Task Delete_WhenIdIsInvalid_DirectionNotExists(long id)
    {
        // Act
        var result = await service.Delete(id).ConfigureAwait(false);

        // Assert
        Assert.False(result.Succeeded);
        Assert.AreEqual(result.OperationResult.Errors.ElementAt(0).Description, $"SubDirection with Id = {id} does not exist.");
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
