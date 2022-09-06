using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.Redis;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class StatisticServiceTest
{
    private IStatisticService service;

    private Mock<IApplicationRepository> applicationRepository;
    private Mock<IWorkshopRepository> workshopRepository;
    private Mock<IEntityRepository<long, Direction>> directionRepository;

    private Mock<IMapper> mapper;
    private Mock<ICacheService> cache;

    [SetUp]
    public void SetUp()
    {
        applicationRepository = new Mock<IApplicationRepository>();
        workshopRepository = new Mock<IWorkshopRepository>();
        directionRepository = new Mock<IEntityRepository<long, Direction>>();
        var ratingService = new Mock<IRatingService>();
        var logger = new Mock<ILogger<StatisticService>>();
        mapper = new Mock<IMapper>();
        cache = new Mock<ICacheService>();

        service = new StatisticService(
            applicationRepository.Object,
            workshopRepository.Object,
            ratingService.Object,
            directionRepository.Object,
            logger.Object,
            mapper.Object,
            cache.Object);
    }

    [Test]
    public async Task GetPopularWorkshops_WhenCityNotQueried_ShouldReturnCertainWorkshops()
    {
        // Arrange
        List<WorkshopCard> expectedWorkshopCards = ExpectedWorkshopCardsNoCityFilter();

        SetupGetPopularWorkshops();

        mapper.Setup(m => m.Map<List<WorkshopCard>>(It.IsAny<List<Workshop>>()))
            .Returns(expectedWorkshopCards);

        // Act
        var result = await service
            .GetPopularWorkshopsFromDatabase(2, 0)
            .ConfigureAwait(false);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                expectedWorkshopCards, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task GetPopularWorkshops_WithCityQueried_ShouldReturnCertainWorkshops()
    {
        // Arrange
        List<Workshop> expectedWorkshops = ExpectedWorkshopCardsCityFilter();

        SetupGetPopularWorkshopsIncludingCATOTTG();

        var expectedWorkshopCards = new List<WorkshopCard>();
        mapper.Setup(m => m.Map<List<WorkshopCard>>(It.IsAny<List<Workshop>>()))
            .Returns(expectedWorkshopCards);

        // Act
        var result = await service
            .GetPopularWorkshopsFromDatabase(2, 31737)
            .ConfigureAwait(false);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                expectedWorkshopCards, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task GetPopularDirections_WhenCityNotQueried_ShouldReturnCertainDirections()
    {
        // Arrange
        List<DirectionDto> expectedDirectionStatistic = ExpectedDirectionStatisticsNoCityFilter();

        SetupGetPopularDirections();

        foreach (var stat in expectedDirectionStatistic)
        {
            mapper.Setup(m => m.Map<DirectionDto>(It.IsAny<Direction>()))
                .Returns(stat);
        }

        // Act
        var result = await service
            .GetPopularDirectionsFromDatabase(1, 0)
            .ConfigureAwait(false);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                expectedDirectionStatistic, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task GetPopularDirections_WithCityQueried_ShouldReturnCertainDirections()
    {
        // Arrange
        List<DirectionDto> expectedDirectionStatistic = ExpectedDirectionStatisticsCityFilter();

        SetupGetPopularDirections();

        foreach (var stat in expectedDirectionStatistic)
        {
            mapper.Setup(m => m.Map<DirectionDto>(It.IsAny<Direction>()))
                .Returns(stat);
        }

        // Act
        var result = await service
            .GetPopularDirectionsFromDatabase(1, 4970)
            .ConfigureAwait(false);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                ExpectedDirectionStatisticsCityFilter(), options => options.WithStrictOrdering());
    }

    [TearDown]
    public void TearDown()
    {
        workshopRepository.Verify();
        applicationRepository.Verify();
        directionRepository.Verify();
    }

    #region Setup

    private void SetupGetPopularWorkshops()
    {
        var workshopsMock = WithWorkshops().AsQueryable().BuildMock();

        workshopRepository
            .Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(workshopsMock)
            .Verifiable();
    }

    private void SetupGetPopularWorkshopsIncludingCATOTTG()
    {
        var workshopsMock = WithWorkshopsIncludingCATOTTG().AsQueryable().BuildMock();

        workshopRepository
            .Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(workshopsMock)
            .Verifiable();
    }

    private void SetupGetPopularDirections()
    {
        var workshopsMock = WithWorkshops().AsQueryable().BuildMock();
        var applicationsMock = WithApplications().AsQueryable().BuildMock();
        var directionsMock = WithDirections().AsQueryable().BuildMock();

        workshopRepository.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(workshopsMock)
            .Verifiable();

        applicationRepository.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(applicationsMock)
            .Verifiable();

        directionRepository.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Direction, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Direction, object>>, SortDirection>>(),
                It.IsAny<bool>()))
            .Returns(directionsMock)
            .Verifiable();
    }

    #endregion

    #region With

    private IEnumerable<Workshop> WithWorkshops()
    {
        return new List<Workshop>
        {
            new Workshop
            {
                Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                Title = "w1",
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                InstitutionHierarchy = new InstitutionHierarchy
                {
                    Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                    Directions = new List<Direction>
                    {
                        new Direction
                        {
                            Id = 1,
                        },
                    },
                },
                Address = new Address
                {
                    CATOTTGId = 4970,
                    CATOTTG = new CATOTTG
                    {
                        Category = "M",
                    },
                },
                Applications = new List<Application>
                {
                    new Application { Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb") },
                },
            },
            new Workshop
            {
                Id = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"),
                Title = "w2",
                InstitutionHierarchyId = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"),
                InstitutionHierarchy = new InstitutionHierarchy
                {
                    Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"),
                    Directions = new List<Direction>
                    {
                        new Direction
                        {
                            Id = 2,
                        },
                    },
                },
                Address = new Address
                {
                    CATOTTGId = 4970,
                    CATOTTG = new CATOTTG
                    {
                        Category = "M",
                    },
                },
                Applications = new List<Application>
                {
                    new Application { Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be") },
                    new Application { Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd") },
                },
            },
            new Workshop
            {
                Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"),
                Title = "w3",
                InstitutionHierarchyId = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"),
                InstitutionHierarchy = new InstitutionHierarchy
                {
                    Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"),
                    Directions = new List<Direction>
                    {
                        new Direction
                        {
                            Id = 3,
                        },
                    },
                },
                Address = new Address
                {
                    CATOTTGId = 5000,
                    CATOTTG = new CATOTTG
                    {
                        Category = "C",
                    },
                },
                Applications = new List<Application>
                {
                    new Application { Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d") },
                    new Application { Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d") },
                    new Application { Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771") },
                },
            },
        };
    }

    private IEnumerable<Workshop> WithWorkshopsIncludingCATOTTG()
    {
        return new List<Workshop>
        {
            new Workshop
            {
                Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                Title = "w1",
                InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                InstitutionHierarchy = new InstitutionHierarchy
                {
                    Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                    Directions = new List<Direction>
                    {
                        new Direction
                        {
                            Id = 1,
                        },
                    },
                },
                Address = new Address
                {
                    CATOTTGId = 31739,
                    CATOTTG = new CATOTTG
                    {
                        Category = "B",
                        ParentId = 31737,
                    },
                },
                Applications = new List<Application>
                {
                    new Application { Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb") },
                },
            },
            new Workshop
            {
                Id = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"),
                Title = "w2",
                InstitutionHierarchyId = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"),
                InstitutionHierarchy = new InstitutionHierarchy
                {
                    Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"),
                    Directions = new List<Direction>
                    {
                        new Direction
                        {
                            Id = 2,
                        },
                    },
                },
                Address = new Address
                {
                    CATOTTGId = 31737,
                    CATOTTG = new CATOTTG
                    {
                        Category = "K",
                        ParentId = null,
                    },
                },
                Applications = new List<Application>
                {
                    new Application { Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be") },
                    new Application { Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd") },
                },
            },
            new Workshop
            {
                Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"),
                Title = "w3",
                InstitutionHierarchyId = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"),
                InstitutionHierarchy = new InstitutionHierarchy
                {
                    Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"),
                    Directions = new List<Direction>
                    {
                        new Direction
                        {
                            Id = 3,
                        },
                    },
                },
                Address = new Address
                {
                    CATOTTGId = 5000,
                    CATOTTG = new CATOTTG
                    {
                        Category = "C",
                        ParentId = 4971,
                    },
                },
                Applications = new List<Application>
                {
                    new Application { Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d") },
                    new Application { Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d") },
                    new Application { Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771") },
                },
            },
        };
    }

    private IEnumerable<Direction> WithDirections()
    {
        return new List<Direction>
        {
            new Direction { Id = 1 },
            new Direction { Id = 2 },
            new Direction { Id = 3 },
        };
    }

    private IEnumerable<Application> WithApplications()
    {
        return new List<Application>
        {
            new Application
            {
                Id = new Guid("0083633f-4e5b-4c09-a89d-52d8a9b89cdb"), WorkshopId = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"), Workshop = new Workshop
                {
                    Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"), InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"), InstitutionHierarchy = new InstitutionHierarchy
                    {
                        Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"),
                        Directions = new List<Direction>
                        {
                            new Direction
                            {
                                Id = 1,
                            },
                        },
                    },
                },
            },
            new Application
            {
                Id = new Guid("7c5f8f7c-d850-44d0-8d4e-fd2de99453be"), WorkshopId = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), Workshop = new Workshop
                {
                    Id = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), InstitutionHierarchyId = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"), InstitutionHierarchy = new InstitutionHierarchy
                    {
                        Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"),
                        Directions = new List<Direction>
                        {
                            new Direction
                            {
                                Id = 2,
                            },
                        },
                    },
                },
            },
            new Application
            {
                Id = new Guid("1745d16a-6181-43d7-97d0-a1d6cc34a8bd"), WorkshopId = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), Workshop = new Workshop
                {
                    Id = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), InstitutionHierarchyId = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"), InstitutionHierarchy = new InstitutionHierarchy
                    {
                        Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"),
                        Directions = new List<Direction>
                        {
                            new Direction
                            {
                            Id = 2,
                            },
                        },
                    },
                },
            },
            new Application
            {
                Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"), WorkshopId = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), Workshop = new Workshop
                {
                    Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"), InstitutionHierarchy = new InstitutionHierarchy
                    {
                        Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"),
                        Directions = new List<Direction>
                        {
                            new Direction
                            {
                                Id = 3,
                            },
                        },
                    },
                },
            },
            new Application
            {
                Id = new Guid("01d08412-69d3-4620-8c54-7b997430e08d"), WorkshopId = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), Workshop = new Workshop
                {
                    Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"), InstitutionHierarchy = new InstitutionHierarchy
                    {
                        Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"),
                        Directions = new List<Direction>
                        {
                            new Direction
                            {
                                Id = 3,
                            },
                        },
                    },
                },
            },
            new Application
            {
                Id = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"), WorkshopId = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), Workshop = new Workshop
                {
                    Id = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), InstitutionHierarchyId = new Guid("af475193-6a1e-4a75-9ba3-439c4300f771"), InstitutionHierarchy = new InstitutionHierarchy
                    {
                        Id = new Guid("af628dd5-e9b6-4ad4-9d12-e87063d8707d"),
                        Directions = new List<Direction>
                        {
                            new Direction
                            {
                                Id = 3,
                            },
                        },
                    },
                },
            },
        };
    }

    private IEnumerable<Workshop> WithWorkshopsFilteredForCityQuery(int limit, string city)
    {
        var workshops = WithWorkshops();

        if (!string.IsNullOrWhiteSpace(city))
        {
            workshops = workshops
                .Where(w => string.Equals(w.Address.CATOTTG.Name, city.Trim()));
        }

        var workshopsWithApplications = workshops.Select(w => new
        {
            Workshop = w,
            Applications = w.Applications.Count,
        });

        var popularWorkshops = workshopsWithApplications
            .OrderByDescending(w => w.Applications)
            .Select(w => w.Workshop)
            .Take(limit);

        return popularWorkshops.ToList();
    }

    #endregion

    #region Expected

    private List<WorkshopCard> ExpectedWorkshopCardsNoCityFilter()
    {
        return new List<WorkshopCard>
        {
            new WorkshopCard {WorkshopId = new Guid("6f8bf795-072d-4fca-ad89-e54a275eb674"), Title = "w3", Address = new AddressDto {CATOTTGId = 5000}},
            new WorkshopCard {WorkshopId = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"), Title = "w2", Address = new AddressDto {CATOTTGId = 4970}},
        };
    }

    private List<Workshop> ExpectedWorkshopCardsCityFilter()
    {
        return new List<Workshop>
        {
            new Workshop
            {
                Id = new Guid("3a2fbb29-e097-4184-ad02-26ed1e5f5057"),
                Title = "w2",
                Address = new Address
                {
                    CATOTTGId = 31737,
                    CATOTTG = new CATOTTG
                    {
                        Category = "K",
                        ParentId = null,
                    },
                },
            },
            new Workshop
            {
                Id = new Guid("953708d7-8c35-4607-bd9b-f034e853bb89"),
                Title = "w1",
                Address = new Address
                {
                    CATOTTGId = 31739,
                    CATOTTG = new CATOTTG
                    {
                        Category = "B",
                        ParentId = 31737,
                    },
                },
            },
        };
    }

    private List<DirectionDto> ExpectedDirectionStatisticsNoCityFilter()
    {
        return new List<DirectionDto>
        {
            new DirectionDto { Id = 3, WorkshopsCount = 1 },
        };
    }

    private List<DirectionDto> ExpectedDirectionStatisticsCityFilter()
    {
        return new List<DirectionDto>
        {
            new DirectionDto { Id = 2, WorkshopsCount = 1 },
        };
    }

    #endregion

}