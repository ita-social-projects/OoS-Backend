using System.Collections.Generic;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Extensions;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class ApplicationExtensionTests
{
    // Delete it
    [Test]
    public void ApplicationExtensions_AmountOfPendingApplications_ReturnValue()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;

        applications.Add(application);

        // Act
        var result = applications.AmountOfPendingApplications();

        // Assert
        Assert.AreEqual(1, result);
    }

    [TestCase(ApplicationStatus.AcceptedForSelection)]
    [TestCase(ApplicationStatus.Approved)]
    [TestCase(ApplicationStatus.StudyingForYears)]
    [TestCase(ApplicationStatus.Completed)]
    [TestCase(ApplicationStatus.Rejected)]
    [TestCase(ApplicationStatus.Left)]
    public void ApplicationExtensions_AmountOfPendingApplications_WrongStatus(ApplicationStatus status)
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = status;

        applications.Add(application);

        // Act
        var result = applications.AmountOfPendingApplications();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_AmountOfPendingApplications_ApplicationIsDeleted()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;
        application.IsDeleted = true;

        applications.Add(application);

        // Act
        var result = applications.AmountOfPendingApplications();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_AmountOfPendingApplications_ChildIsNull()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;

        applications.Add(application);

        // Act
        var result = applications.AmountOfPendingApplications();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_AmountOfPendingApplications_ParentIsNull()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate());
        application.Status = ApplicationStatus.Pending;

        applications.Add(application);

        // Act
        var result = applications.AmountOfPendingApplications();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_AmountOfPendingApplications_ChildIsDeleted()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;

        application.Child.IsDeleted = true;

        applications.Add(application);

        // Act
        var result = applications.AmountOfPendingApplications();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_AmountOfPendingApplications_ParentIsDeleted()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Pending;

        application.Parent.IsDeleted = true;

        applications.Add(application);

        // Act
        var result = applications.AmountOfPendingApplications();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_TakenSeats_ReturnValue()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Approved;

        applications.Add(application);

        application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.StudyingForYears;

        applications.Add(application);

        // Act
        var result = applications.TakenSeats();

        // Assert
        Assert.AreEqual(2, result);
    }

    [TestCase(ApplicationStatus.AcceptedForSelection)]
    [TestCase(ApplicationStatus.Pending)]
    [TestCase(ApplicationStatus.Completed)]
    [TestCase(ApplicationStatus.Rejected)]
    [TestCase(ApplicationStatus.Left)]
    public void ApplicationExtensions_TakenSeats_WrongStatus(ApplicationStatus status)
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = status;

        applications.Add(application);

        // Act
        var result = applications.TakenSeats();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_TakenSeats_ApplicationIsDeleted()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Approved;
        application.IsDeleted = true;

        applications.Add(application);

        // Act
        var result = applications.TakenSeats();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_TakenSeats_ChildIsNull()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Approved;

        applications.Add(application);

        // Act
        var result = applications.TakenSeats();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_TakenSeats_ChildIsDeleted()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Approved;

        application.Child.IsDeleted = true;

        applications.Add(application);

        // Act
        var result = applications.TakenSeats();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_TakenSeats_ParentIsNull()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate());
        application.Status = ApplicationStatus.Approved;

        applications.Add(application);

        // Act
        var result = applications.TakenSeats();

        // Assert
        Assert.AreEqual(0, result);
    }

    [Test]
    public void ApplicationExtensions_TakenSeats_ParentIsDeleted()
    {
        // Arrange
        var applications = new List<Application>();

        var application = ApplicationGenerator.Generate().WithChild(ChildGenerator.Generate()).WithParent(ParentGenerator.Generate());
        application.Status = ApplicationStatus.Approved;

        application.Parent.IsDeleted = true;

        applications.Add(application);

        // Act
        var result = applications.TakenSeats();

        // Assert
        Assert.AreEqual(0, result);
    }
}
