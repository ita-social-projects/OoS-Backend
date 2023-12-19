using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.IntegrationTests.MappingIntergrationTests;

[TestFixture]
public class WorkshopMappingTests
{
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        this.mapper = new Mapper(config);
    }

    [Test]
    public void Map_WorkshopToWorkshopProviderViewCard_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var workshop = new Workshop
        {
            Applications = new List<Application>
            {
                new Application { Status = ApplicationStatus.Pending, IsDeleted = false },
                new Application { Status = ApplicationStatus.Pending, IsDeleted = true },
                new Application { Status = ApplicationStatus.Approved },
                new Application { Status = ApplicationStatus.StudyingForYears },
            },
        };

        // Act
        var result = mapper.Map<WorkshopProviderViewCard>(workshop);

        // Assert
        Assert.AreEqual(
            workshop.Applications.Count(x => x.Status == ApplicationStatus.Pending && !x.IsDeleted),
            result.AmountOfPendingApplications);
        Assert.AreEqual(
            workshop.Applications.Count(x => x.Status == ApplicationStatus.Approved
                                                        || x.Status == ApplicationStatus.StudyingForYears),
            result.TakenSeats);
        Assert.AreEqual(0, result.UnreadMessages);
    }
}
