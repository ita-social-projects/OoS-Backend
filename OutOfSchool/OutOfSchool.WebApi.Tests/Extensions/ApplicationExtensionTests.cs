using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class ApplicationExtensionTests
{
    [Test]
    public void ApplicationExtensions_AmountOfPendingApplications_ReturnValue()
    {
        // Arrange
        var applications = GetApplications();

        // Act
        var result = applications.AmountOfPendingApplications();
        var expected = applications.Count(x =>
            x.Status == ApplicationStatus.Pending
            && !x.IsDeleted
            && (x.Child == null || !x.Child.IsDeleted)
            && (x.Parent == null || !x.Parent.IsDeleted));

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void ApplicationExtensions_TakenSeats_ReturnValue()
    {
        // Arrange
        var applications = GetApplications();

        // Act
        var result = applications.TakenSeats();
        var expected = applications.Count(x =>
            (x.Status == ApplicationStatus.Approved || x.Status == ApplicationStatus.StudyingForYears)
            && !x.IsDeleted
            && (x.Child == null || !x.Child.IsDeleted)
            && (x.Parent == null || !x.Parent.IsDeleted));

        // Assert
        Assert.AreEqual(expected, result);
    }

    private List<Application> GetApplications()
    {
        var result = new List<Application>();

        // 1
        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;

        result.Add(application);

        // 2
        application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;

        result.Add(application);

        // 3
        application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;
        application.IsDeleted = true;

        result.Add(application);

        // 4
        application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.StudyingForYears;

        result.Add(application);

        // 5
        application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;
        application.Child.IsDeleted = true;

        result.Add(application);

        // 6
        application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;
        application.Parent.IsDeleted = true;

        result.Add(application);

        // 7
        application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Approved;

        result.Add(application);

        // 8
        application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Approved;
        application.IsDeleted= true;

        result.Add(application);

        return result;
    }
}
