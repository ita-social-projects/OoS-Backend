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
using OutOfSchool.BusinessLogic.Models.Achievement;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class AchievementServiceTest
{
    private readonly Guid validWorkshopGuid = new Guid("08da8474-a754-4d37-879e-4932d389b27a");
    private readonly Guid notValidWorkshopGuid = new Guid("05da8774-d754-1d37-979e-4135d389c27a");
    private DbContextOptions<OutOfSchoolDbContext> options;
    private OutOfSchoolDbContext context;
    private AchievementService service;
    private IAchievementRepository achievementRepository;
    private Mock<ILogger<AchievementService>> logger;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);

        achievementRepository = new AchievementRepository(context);
        logger = new Mock<ILogger<AchievementService>>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        service = new AchievementService(achievementRepository, logger.Object, localizer.Object, mapper);

        SeedDatabase();
    }

    [Test]
    public async Task GetByFilter_Valid_ReturnsSearchResult()
    {
        // Arrange
        var expected = Achievements();
        var filter = new AchievementsFilter() { WorkshopId = validWorkshopGuid };

        // Act
        var result = await service.GetByFilter(filter);

        // Assert
        Assert.IsNotNull(result.Entities);
        Assert.AreEqual(expected.First().Id, result.Entities.First().Id);
        Assert.AreEqual(expected.Count, result.TotalAmount);
        Assert.IsInstanceOf<IReadOnlyCollection<AchievementDto>>(result.Entities);
    }

    [Test]
    public async Task GetByFilter_NotValidWorkshopId_ReturnsEmpty()
    {
        // Arrange
        var expexted = new List<Achievement>();
        var filter = new AchievementsFilter() { WorkshopId = notValidWorkshopGuid };

        // Act
        var result = await service.GetByFilter(filter);

        // Assert
        Assert.That(result.Entities, Is.Empty);
        Assert.AreEqual(result.TotalAmount, expexted.Count);
    }

    private void SeedDatabase()
    {
        using var ctx = new TestOutOfSchoolDbContext(options);
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            ctx.Achievements.AddRange(Achievements());

            ctx.SaveChanges();
        }
    }

    private List<Achievement> Achievements()
    {
        return new List<Achievement>()
        {
            new Achievement()
            {
                AchievementDate = DateTime.Now,
                AchievementTypeId = 1,
                Id = validWorkshopGuid,
                WorkshopId = new Guid("08da8474-a754-4d37-879e-4932d389b27a"),
                Title = "Назва1",
                Children = new List<Child>()
                {
                    new Child
                    {
                        DateOfBirth = DateTime.Now,
                        FirstName = "Арієль",
                        Gender = Gender.Female,
                        Id = new Guid("0d3a847c-2f7a-482f-89f2-417b98c6d02a"),
                        IsParent = true,
                        LastName = "Русалонька",
                        MiddleName = "Моряка",
                        Parent = new Parent
                        {
                            User = new User
                            {
                                Email = "sea@gmail.com",
                                FirstName = "Арієль",
                                Id = "08da847c-2f6d-4327-8184-b1a11c8f7008",
                                LastName = "Русалонька",
                                MiddleName = "Моряка",
                                PhoneNumber = "498344943",
                            },
                            Gender = Gender.Female,
                            DateOfBirth = DateTime.Now,
                            UserId = "06da847c-2f6d-4327-8184-b1a11c8f7008",
                            Id = new Guid("05da847c-2f6d-4327-8184-b1a11c8f7008"),
                        },
                        ParentId = new Guid("05da847c-2f6d-4327-8184-b1a11c8f7008"),
                        PlaceOfStudy = null,
                        SocialGroups = new List<SocialGroup>(),
                    },
                    new Child()
                    {
                        DateOfBirth = DateTime.Now,
                        FirstName = "Валентина",
                        Gender = Gender.Female,
                        Id = new Guid("08da8b0f-fccd-496a-8288-b02729234229"),
                        IsParent = true,
                        LastName = "Русалонька",
                        MiddleName = "Моряка",
                        Parent = new Parent
                        {
                            User = new User
                            {
                                Email = "sea@gmail.com",
                                FirstName = "Арієль",
                                Id = "09da847c-2f6d-5327-8184-b1a11c8f7808",
                                LastName = "Русалонька",
                                MiddleName = "Моряка",
                                PhoneNumber = "498344943",
                            },
                            Gender = Gender.Female,
                            DateOfBirth = DateTime.Now,
                            UserId = "09da847c-2f6d-5327-8184-b1a11c8f7808",
                            Id = new Guid("09da847c-2f6d-5327-8184-b1a11c8f7808"),
                        },
                        ParentId = new Guid("09da847c-2f6d-5327-8184-b1a11c8f7808"),
                        PlaceOfStudy = null,
                        SocialGroups = new List<SocialGroup>(),
                    },
                },
            },
            new Achievement()
            {
                AchievementDate = DateTime.Now,
                AchievementTypeId = 1,
                WorkshopId = validWorkshopGuid,
                Id = new Guid("08da8b47-f4c8-46f7-8376-3aa4d947a1e1"),
                Title = "Назва2",
                Children = new List<Child>()
                {
                    new Child
                    {
                        DateOfBirth = DateTime.Now,
                        FirstName = "Арієль",
                        Gender = Gender.Female,
                        Id = new Guid("08da847c-2f7a-482f-89f2-417b98c6d02a"),
                        IsParent = true,
                        LastName = "Русалонька",
                        MiddleName = "Моряка",
                        Parent = new Parent
                        {
                            User = new User
                            {
                                Email = "sea@gmail.com",
                                FirstName = "Арієль",
                                Id = "06da847c-2f6d-4327-8184-b1a11c8f7008",
                                LastName = "Русалонька",
                                MiddleName = "Моряка",
                                PhoneNumber = "498344943",
                            },
                            Gender = Gender.Female,
                            DateOfBirth = DateTime.Now,
                            UserId = "06da847c-2f6d-4327-8184-b1a11c8f7008",
                            Id = new Guid("02da847c-2f6d-4327-8184-b1a11c8f7008"),
                        },
                        ParentId = new Guid("02da847c-2f6d-4327-8184-b1a11c8f7008"),
                        PlaceOfStudy = null,
                        SocialGroups = new List<SocialGroup>(),
                    },
                    new Child()
                    {
                        DateOfBirth = DateTime.Now,
                        FirstName = "Валентина",
                        Gender = Gender.Female,
                        Id = new Guid("08da8b0f-fccd-436a-8288-b02829234229"),
                        IsParent = true,
                        LastName = "Русалонька",
                        MiddleName = "Моряка",
                        Parent = new Parent
                        {
                            User = new User
                            {
                                Email = "sea@gmail.com",
                                FirstName = "Арієль",
                                Id = "03da847c-2f6d-4327-8184-b1a11c8f7008",
                                LastName = "Русалонька",
                                MiddleName = "Моряка",
                                PhoneNumber = "498344943",
                            },
                            DateOfBirth = DateTime.Now,
                            Gender = Gender.Female,
                            UserId = "578b8827-9985-4839-a3ab-258abef30a54",
                        },
                        ParentId = new Guid("03da847c-2f6d-4327-8184-b1a11c8f7008"),
                        PlaceOfStudy = null,
                        SocialGroups = new List<SocialGroup>(),
                    },
                },
            },
        };
    }
}
