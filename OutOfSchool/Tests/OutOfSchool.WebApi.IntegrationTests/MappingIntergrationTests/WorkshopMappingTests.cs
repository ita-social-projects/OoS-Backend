using System.Collections.Generic;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.IntegrationTests.MappingIntergrationTests;

[TestFixture]
public class WorkshopMappingTests
{
    [Test]
    public void Map_WorkshopToWorkshopProviderViewCard_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var workshop = new Workshop
        {
            Applications = new List<Application>
            {
                new() { Status = ApplicationStatus.Pending, IsDeleted = false },
                new() { Status = ApplicationStatus.Pending, IsDeleted = true },
                new() { Status = ApplicationStatus.Approved },
                new() { Status = ApplicationStatus.StudyingForYears },
            },
        };

        // Act
        var result = workshop.ToProviderViewCard();

        // Assert
        Assert.AreEqual(0, result.UnreadMessages);
    }
}
