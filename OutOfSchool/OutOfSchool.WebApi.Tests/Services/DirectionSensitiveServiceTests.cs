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
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class DirectionSensitiveServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private IEntityRepositorySoftDeleted<long, Direction> repo;
    private IWorkshopRepository repositoryWorkshop;
    private ISensitiveDirectionService service;
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
        context = new OutOfSchoolDbContext(options);

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
    [Order(6)]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var changedEntity = new DirectionDto()
        {
            Id = 1,
            Title = "ChangedTitle1",
        };

        var expected = await repo.GetById(changedEntity.Id).ConfigureAwait(false);

        mapper.Setup(m => m.Map<Direction>(changedEntity)).Returns(expected);
        mapper.Setup(m => m.Map<DirectionDto>(expected)).Returns(changedEntity);

        // Act
        var result = await service.Update(changedEntity).ConfigureAwait(false);

        // Assert
        Assert.That(changedEntity.Title, Is.EqualTo(result.Title));
    }

    [Test]
    [Order(7)]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var changedEntity = new DirectionDto()
        {
            Title = "NewTitle1",
        };
        var expected = new Direction()
        {
            Title = "NewTitle1",
        };
        mapper.Setup(m => m.Map<Direction>(changedEntity)).Returns(expected);

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedEntity).ConfigureAwait(false));
    }

    private void SeedDatabase()
    {
        using var ctx = new OutOfSchoolDbContext(options);
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