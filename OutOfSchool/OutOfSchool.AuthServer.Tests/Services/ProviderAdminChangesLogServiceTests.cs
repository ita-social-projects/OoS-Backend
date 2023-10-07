using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.AuthServer.Tests.Services;

[TestFixture]
public class ProviderAdminChangesLogServiceTests
{
    private IProviderAdminChangesLogService providerAdminChangesLogService;
    private Mock<IEntityAddOnlyRepository<long, ProviderAdminChangesLog>> providerAdminChangesLogRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        providerAdminChangesLogRepositoryMock = new Mock<IEntityAddOnlyRepository<long, ProviderAdminChangesLog>>();
        providerAdminChangesLogService =
            new ProviderAdminChangesLogService(providerAdminChangesLogRepositoryMock.Object);
    }

    #region SaveChangesLogAsync
    [Test]
    public async Task SaveChangesLogAsync_WhenEntityValid_ShouldSaveLogItem()
    {
        // Arrange
        var providerAdmin = new ProviderAdmin()
        {
            UserId = Guid.NewGuid().ToString(),
            ProviderId = Guid.NewGuid(),
        };
        var expectedResult = new ProviderAdminChangesLog()
        {
            ProviderAdminUserId = providerAdmin.UserId,
            ProviderId = providerAdmin.ProviderId,
        };
        var userId = Guid.NewGuid().ToString();
        var operationType = OperationType.Create;
        var propertyName = "FirstName";
        var oldValue = string.Empty;
        var newValue = "John";

        providerAdminChangesLogRepositoryMock.Setup(m => m.Create(It.IsAny<ProviderAdminChangesLog>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await providerAdminChangesLogService.SaveChangesLogAsync(
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
        ProviderAdmin providerAdmin = null;
        var userId = Guid.NewGuid().ToString();
        var operationType = OperationType.Create;
        var propertyName = "FirstName";
        var oldValue = string.Empty;
        var newValue = "John";
        
        // Act
        Func<Task> action = async () => await providerAdminChangesLogService.SaveChangesLogAsync(
            providerAdmin,
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
