using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;

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
