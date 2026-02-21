using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.Redis;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class StatisticServiceTest
{
    private IStatisticService service;

    private Mock<IApplicationRepository> applicationRepository;
    private Mock<IWorkshopRepository> workshopRepository;
    private Mock<ICompetitiveEventRepository> competitiveEventRepository;
    private Mock<IEntityRepositorySoftDeleted<long, Direction>> directionRepository;
    private Mock<ICacheService> cache;
    private Mock<IAverageRatingService> averageRatingServiceMock;

    [SetUp]
    public void SetUp()
    {
        applicationRepository = new Mock<IApplicationRepository>();
        workshopRepository = new Mock<IWorkshopRepository>();
        competitiveEventRepository = new Mock<ICompetitiveEventRepository>();
        directionRepository = new Mock<IEntityRepositorySoftDeleted<long, Direction>>();
        var logger = new Mock<ILogger<StatisticService>>();
        cache = new Mock<ICacheService>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();

        service = new StatisticService(
            applicationRepository.Object,
            workshopRepository.Object,
            competitiveEventRepository.Object,
            directionRepository.Object,
            logger.Object,
            cache.Object,
            averageRatingServiceMock.Object);
    }

    [Test]
    public async Task GetPopularWorkshops_WhenCityNotQueried_ShouldReturnCertainWorkshops()
    {
        // Arrange
        var workshopsMock = WithWorkshops().AsQueryable().BuildMock();

        workshopRepository
            .Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>()))
            .Returns(workshopsMock)
            .Verifiable();

        var expectedWorkshopCardsIds = new List<Guid>() { new ("6f8bf795-072d-4fca-ad89-e54a275eb674"), new ("3a2fbb29-e097-4184-ad02-26ed1e5f5057") };
        var ratings = RatingsGenerator.GetAverageRatings(expectedWorkshopCardsIds);

        averageRatingServiceMock
            .Setup(r => r.GetByEntityIdsAsync(It.Is<List<Guid>>(l => l.Count == expectedWorkshopCardsIds.Count && !l.Except(expectedWorkshopCardsIds).Any())))
            .ReturnsAsync(ratings);

        // Act
        var result = await service
            .GetPopularWorkshopsFromDatabase(2, 0)
            .ConfigureAwait(false);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                workshopsMock.Where(w => expectedWorkshopCardsIds.Contains(w.Id)).ToCard());
    }

    [Test]
    public async Task GetPopularWorkshops_WithCityQueried_ShouldReturnCertainWorkshops()
    {
        // Arrange
        var workshopsMock = WithWorkshopsIncludingCATOTTG().AsQueryable().BuildMock();

        workshopRepository
            .Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>()))
            .Returns(workshopsMock)
            .Verifiable();

        const int catottgId = 31737;
        var expectedWorkshopCards = workshopsMock
            .Where(w => w.Contacts.Any(c => c.Address.CATOTTGId == catottgId || c.Address.CATOTTG.ParentId == catottgId))
            .ToCard();

        // Act

        var result = await service
            .GetPopularWorkshopsFromDatabase(2, catottgId)
            .ConfigureAwait(false);

        // Assert
        result.Should().BeEquivalentTo(expectedWorkshopCards);
    }

    [Test]
    public async Task GetPopularDirections_WhenCityNotQueried_ShouldReturnCertainDirections()
    {
        // Arrange
        var expectedDirectionStatistic = new List<DirectionDto>
        {
            new() { Id = 3, WorkshopsCount = 1 },
        };

        SetupGetPopularDirections();

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
        var expectedDirectionStatistic = new List<DirectionDto>
        {
            new() { Id = 1, WorkshopsCount = 1 },
        };

        SetupGetPopularDirections();

        // Act
        var result = await service
            .GetPopularDirectionsFromDatabase(1, 4970)
            .ConfigureAwait(false);

        // Assert
        result
            .Should()
            .BeEquivalentTo(expectedDirectionStatistic, options => options.WithStrictOrdering());
    }

    [TearDown]
    public void TearDown()
    {
        workshopRepository.Verify();
        applicationRepository.Verify();
        directionRepository.Verify();
    }

    #region Setup

    private void SetupGetPopularDirections()
    {
        var workshopsMock = WithWorkshops().AsQueryable().BuildMock();
        var applicationsMock = WithApplications().AsQueryable().BuildMock();
        var directionsMock = WithDirections().AsQueryable().BuildMock();

        workshopRepository.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Workshop, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Workshop, object>>, SortDirection>>()))
            .Returns(workshopsMock)
            .Verifiable();

        applicationRepository.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Application, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Application, object>>, SortDirection>>()))
            .Returns(applicationsMock)
            .Verifiable();

        directionRepository.Setup(w => w.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Direction, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Direction, object>>, SortDirection>>()))
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
                    SubDirections = new List<SubDirection>
                    {
                        new SubDirection
                        {
                            Id = 1,
                            DirectionId = 1,
                            Direction = new Direction
                            {
                                Id = 1
                            }
                        },
                    },
                    InstitutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"),
                },
                Contacts = [
                    new ()
                    {
                        Title = "Test",
                        IsDefault = true,
                        Address = new ()
                        {
                            CATOTTGId = 4970,
                            CATOTTG = new CATOTTG
                            {
                                Category = "M",
                            },
                        },
                    }
                ],
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
                    SubDirections = new List<SubDirection>
                    {
                        new SubDirection
                        {
                            Id = 2,
                            DirectionId = 2,
                            Direction = new Direction
                            {
                                Id = 2
                            }
                        },
                    },
                    InstitutionId = Guid.NewGuid(),
                },
                Contacts = [
                    new ()
                    {
                        Title = "Test",
                        IsDefault = true,
                        Address = new ()
                        {
                            CATOTTGId = 4970,
                            CATOTTG = new CATOTTG
                            {
                                Category = "M",
                            },
                        }
                    }
                ],
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
                    SubDirections = new List<SubDirection>
                    {
                        new SubDirection
                        {
                            Id = 3,
                            DirectionId = 3,
                            Direction = new Direction
                            {
                                Id = 3
                            }
                        },
                    },
                    InstitutionId = Guid.NewGuid(),
                },
                Contacts = [
                    new ()
                    {
                        Title = "Test",
                        IsDefault = true,
                        Address = new ()
                        {
                            CATOTTGId = 5000,
                            CATOTTG = new CATOTTG
                            {
                                Category = "C",
                            },
                        }
                    }
                ],
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
                    SubDirections = new List<SubDirection>
                    {
                        new SubDirection
                        {
                            Id = 1,
                            DirectionId = 1,
                            Direction = new Direction
                            {
                                Id = 1
                            }
                        },
                    },
                },
                Contacts = [
                    new ()
                    {
                        Title = "Test",
                        IsDefault = true,
                        Address = new ()
                        {
                            CATOTTGId = 31739,
                            CATOTTG = new CATOTTG
                            {
                                Category = "B",
                                ParentId = 31737,
                            },
                        }
                    }
                ],
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
                    SubDirections = new List<SubDirection>
                    {
                        new SubDirection
                        {
                            Id = 2,
                            DirectionId = 2,
                            Direction = new Direction
                            {
                                Id = 2
                            }
                        },
                    },
                },
                Contacts = [
                    new ()
                    {
                        Title = "Test",
                        IsDefault = true,
                        Address = new ()
                        {
                            CATOTTGId = 31737,
                            CATOTTG = new CATOTTG
                            {
                                Category = "K",
                                ParentId = null,
                            },
                        }
                    }
                ],
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
                    SubDirections = new List<SubDirection>
                    {
                        new SubDirection
                        {
                            Id = 3,
                            DirectionId = 3,
                            Direction = new Direction
                            {
                                Id = 3
                            }
                        },
                    },
                },
                Contacts = [
                    new ()
                    {
                        Title = "Test",
                        IsDefault = true,
                        Address = new ()
                        {
                            CATOTTGId = 5000,
                            CATOTTG = new CATOTTG
                            {
                                Category = "C",
                                ParentId = 4971,
                            },
                        }
                    }
                ],
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
                        SubDirections = new List<SubDirection>
                        {
                            new SubDirection
                            {
                                Id = 1,
                                DirectionId = 1,
                                Direction = new Direction
                                {
                                    Id = 1
                                }
                            },
                        },
                        InstitutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4"),
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
                        SubDirections = new List<SubDirection>
                        {
                            new SubDirection
                            {
                                Id = 2,
                                DirectionId = 2,
                                Direction = new Direction
                                {
                                    Id = 2
                                }
                            },
                        },
                        InstitutionId = Guid.NewGuid(),
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
                        SubDirections = new List<SubDirection>
                        {
                            new SubDirection
                            {
                            Id = 2,
                            },
                        },
                        InstitutionId = Guid.NewGuid(),
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
                        SubDirections = new List<SubDirection>
                        {
                            new SubDirection
                            {
                                Id = 3,
                                DirectionId = 3,
                                Direction = new Direction
                                {
                                    Id = 3
                                }
                            },
                        },
                        InstitutionId = Guid.NewGuid(),
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
                        SubDirections = new List<SubDirection>
                        {
                            new SubDirection
                            {
                                Id = 3,
                                DirectionId = 3,
                                Direction = new Direction
                                {
                                    Id = 3
                                }
                            },
                        },
                        InstitutionId = Guid.NewGuid(),
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
                        SubDirections = new List<SubDirection>
                        {
                            new SubDirection
                            {
                                Id = 3,
                                DirectionId = 3,
                                Direction = new Direction
                                {
                                    Id = 3
                                }
                            },
                        },
                        InstitutionId = Guid.NewGuid(),
                    },
                },
            },
        };
    }

    #endregion

}