using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class OfficialChangesLogServiceTests
{
    private IOfficialChangesLogService officialChangesLogService;
    private Mock<IEntityAddOnlyRepository<long, EmployeeChangesLog>> providerAdminChangesLogRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        providerAdminChangesLogRepositoryMock = new Mock<IEntityAddOnlyRepository<long, EmployeeChangesLog>>();
        officialChangesLogService =
            new OfficialChangesLogService(providerAdminChangesLogRepositoryMock.Object);
    }

    #region SaveChangesLogAsync
    [Test]
    public async Task SaveChangesLogAsync_WhenEntityValid_ShouldSaveLogItem()
    {
        // Arrange
        var official = new Official
        {
            Individual = new Individual
            {
                UserId = Guid.NewGuid().ToString()
            },
            Position = new Position
            {
                ProviderId = Guid.NewGuid(),
            }
        };
        var expectedResult = new EmployeeChangesLog()
        {
            EmployeeUserId = Guid.NewGuid().ToString(),
            ProviderId = Guid.NewGuid(),
        };
        var userId = Guid.NewGuid().ToString();
        var operationType = OperationType.Create;
        var propertyName = "FirstName";
        var oldValue = string.Empty;
        var newValue = "John";

        providerAdminChangesLogRepositoryMock.Setup(m => m.Create(It.IsAny<EmployeeChangesLog>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await officialChangesLogService.SaveChangesLogAsync(
            official,
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
        Official official = null;
        var userId = Guid.NewGuid().ToString();
        var operationType = OperationType.Create;
        var propertyName = "FirstName";
        var oldValue = string.Empty;
        var newValue = "John";
        
        // Act
        Func<Task> action = async () => await officialChangesLogService.SaveChangesLogAsync(
            official,
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
