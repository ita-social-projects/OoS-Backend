using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.AuthServer.Tests.Services;

[TestFixture]
public class EmployeeChangesLogServiceTests
{
    private IEmployeeChangesLogService employeeChangesLogService;
    private Mock<IEntityAddOnlyRepository<long, EmployeeChangesLog>> providerAdminChangesLogRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        providerAdminChangesLogRepositoryMock = new Mock<IEntityAddOnlyRepository<long, EmployeeChangesLog>>();
        employeeChangesLogService =
            new EmployeeChangesLogService(providerAdminChangesLogRepositoryMock.Object);
    }

    #region SaveChangesLogAsync
    [Test]
    public async Task SaveChangesLogAsync_WhenEntityValid_ShouldSaveLogItem()
    {
        // Arrange
        var providerAdmin = new Employee()
        {
            UserId = Guid.NewGuid().ToString(),
            ProviderId = Guid.NewGuid(),
        };
        var expectedResult = new EmployeeChangesLog()
        {
            EmployeeUserId = providerAdmin.UserId,
            ProviderId = providerAdmin.ProviderId,
        };
        var userId = Guid.NewGuid().ToString();
        var operationType = OperationType.Create;
        var propertyName = "FirstName";
        var oldValue = string.Empty;
        var newValue = "John";

        providerAdminChangesLogRepositoryMock.Setup(m => m.Create(It.IsAny<EmployeeChangesLog>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await employeeChangesLogService.SaveChangesLogAsync(
            providerAdmin,
            userId,
            operationType,
            propertyName,
            oldValue,
            newValue);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task SaveChangesLogAsync_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        Employee employee = null;
        var userId = Guid.NewGuid().ToString();
        var operationType = OperationType.Create;
        var propertyName = "FirstName";
        var oldValue = string.Empty;
        var newValue = "John";
        
        // Act
        Func<Task> action = async () => await employeeChangesLogService.SaveChangesLogAsync(
            employee,
            userId,
            operationType,
            propertyName,
            oldValue,
            newValue);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }
    #endregion
}
