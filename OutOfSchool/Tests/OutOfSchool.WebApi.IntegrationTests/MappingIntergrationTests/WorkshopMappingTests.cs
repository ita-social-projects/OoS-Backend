using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.Mapping;

namespace OutOfSchool.WebApi.IntegrationTests.MappingIntergrationTests;

[TestFixture]
public class WorkshopMappingTests
{
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        this.mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
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
        Assert.AreEqual(0, result.UnreadMessages);
    }
}
