using System.Threading.Tasks;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common.StatusPermissions;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class ApplicationStatusPermissionsTest
{
    private ApplicationStatusPermissions _applicationStatusPermissions;

    [TestCase("parent")]
    [TestCase("provider")]
    [TestCase("techadmin")]
    [TestCase("ministryadmin")]
    public async Task DenyPermission_FromCompletedRejectedLeftToApproved_ReturnsFalse(string role)
    {
        // Arrange default permissions
        _applicationStatusPermissions = new ApplicationStatusPermissions();
        _applicationStatusPermissions.InitDefaultPermissions();

        // Act
        var resultCompleted = _applicationStatusPermissions.CanChangeStatus(role, ApplicationStatus.Completed, ApplicationStatus.Approved);
        var resultRejected = _applicationStatusPermissions.CanChangeStatus(role, ApplicationStatus.Rejected, ApplicationStatus.Approved);
        var resultLeft = _applicationStatusPermissions.CanChangeStatus(role, ApplicationStatus.Left, ApplicationStatus.Approved);

        // Assert
        Assert.AreEqual(false, resultCompleted);
        Assert.AreEqual(false, resultRejected);
        Assert.AreEqual(false, resultLeft);
    }

    [TestCase("parent")]
    [TestCase("provider")]
    public async Task DenyPermission_FromAllToAcceptedToSelectionWithoutCompetitiveSelection_ReturnsValidResults(string role)
    {
        // Arrange permissions
        _applicationStatusPermissions = new ApplicationStatusPermissions();
        _applicationStatusPermissions.InitDefaultPermissions();

        // Act
        var result = _applicationStatusPermissions.CanChangeStatus(role, ApplicationStatus.Pending, ApplicationStatus.AcceptedForSelection);

        // Assert
        Assert.AreEqual(false, result);
    }

    [Test]
    public async Task ProviderPermission_FromPendingToAllWithoutCompetitiveSelection_ReturnsValidResults()
    {
        // Arrange permissions
        _applicationStatusPermissions = new ApplicationStatusPermissions();
        _applicationStatusPermissions.InitDefaultPermissions();

        // Act
        var toAcceptedForSelectionResult = _applicationStatusPermissions.CanChangeStatus("provider", ApplicationStatus.Pending, ApplicationStatus.AcceptedForSelection);
        var toApprovedResult = _applicationStatusPermissions.CanChangeStatus("provider", ApplicationStatus.Pending, ApplicationStatus.Approved);
        var toStudyingForYearsResult = _applicationStatusPermissions.CanChangeStatus("provider", ApplicationStatus.Pending, ApplicationStatus.StudyingForYears);
        var toCompletedResult = _applicationStatusPermissions.CanChangeStatus("provider", ApplicationStatus.Pending, ApplicationStatus.Completed);
        var toRejectedResult = _applicationStatusPermissions.CanChangeStatus("provider", ApplicationStatus.Pending, ApplicationStatus.Rejected);
        var toLeftResult = _applicationStatusPermissions.CanChangeStatus("provider", ApplicationStatus.Pending, ApplicationStatus.Left);

        // Assert
        Assert.AreEqual(false, toAcceptedForSelectionResult);
        Assert.AreEqual(true, toApprovedResult);
        Assert.AreEqual(true, toStudyingForYearsResult);
        Assert.AreEqual(true, toCompletedResult);
        Assert.AreEqual(true, toRejectedResult);
        Assert.AreEqual(true, toLeftResult);
    }

    [Test]
    public async Task ProviderPermission_FromPendingToAllWithCompetitiveSelection_ReturnsValidResults()
    {
        _applicationStatusPermissions = new ApplicationStatusPermissions();
        _applicationStatusPermissions.InitCompetitiveSelectionPermissions();
        var from = ApplicationStatus.Pending;

        // Act
        var toAcceptedForSelectionResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.AcceptedForSelection);
        var toApprovedResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Approved);
        var toStudyingForYearsResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.StudyingForYears);
        var toCompletedResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Completed);
        var toRejectedResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Rejected);
        var toLeftResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Left);

        // Assert
        Assert.AreEqual(true, toAcceptedForSelectionResult);
        Assert.AreEqual(false, toApprovedResult);
        Assert.AreEqual(false, toStudyingForYearsResult);
        Assert.AreEqual(false, toCompletedResult);
        Assert.AreEqual(false, toRejectedResult);
        Assert.AreEqual(false, toLeftResult);
    }

    [Test]
    public async Task ProviderPermission_FromAcceptedForSelectionToAllWithCompetitiveSelection_ReturnsValidResults()
    {
        _applicationStatusPermissions = new ApplicationStatusPermissions();
        _applicationStatusPermissions.InitCompetitiveSelectionPermissions();
        var from = ApplicationStatus.AcceptedForSelection;

        // Act
        var toPendingResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Pending);
        var toApprovedResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Approved);
        var toStudyingForYearsResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.StudyingForYears);
        var toCompletedResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Completed);
        var toRejectedResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Rejected);
        var toLeftResult = _applicationStatusPermissions.CanChangeStatus("provider", from, ApplicationStatus.Left);

        // Assert
        Assert.AreEqual(true, toPendingResult);
        Assert.AreEqual(true, toApprovedResult);
        Assert.AreEqual(true, toStudyingForYearsResult);
        Assert.AreEqual(true, toCompletedResult);
        Assert.AreEqual(true, toRejectedResult);
        Assert.AreEqual(true, toLeftResult);
    }
}