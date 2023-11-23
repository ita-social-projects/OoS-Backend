using FluentAssertions;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Services;
using System;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ParentBlockedByAdminLogServiceTests
{
    private IParentBlockedByAdminLogService parentBlockedByAdminLogService;
    private Mock<IEntityAddOnlyRepository<long, ParentBlockedByAdminLog>> parentBlockedByAdminLogRepositoryMock;

    [SetUp]
    public void SetUp()
    {
        parentBlockedByAdminLogRepositoryMock =
            new Mock<IEntityAddOnlyRepository<long, ParentBlockedByAdminLog>>();
        parentBlockedByAdminLogService =
            new ParentBlockedByAdminLogService(parentBlockedByAdminLogRepositoryMock.Object);
    }

    #region SaveChangesLogAsync
    [Test]
    public async Task SaveChangesLogAsync_WhenArgumentsValid_ShouldSaveLogItemReturn1()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var operationDate = DateTime.Now;
        var reason = "Reason to block parent";
        var isBlocked = true;

        var expectedResult = new ParentBlockedByAdminLog()
        {
            ParentId = parentId,
            UserId = userId,
            OperationDate = operationDate,
            Reason = reason,
            IsBlocked = isBlocked,
        };
        parentBlockedByAdminLogRepositoryMock.Setup(x => x.Create(It.IsAny<ParentBlockedByAdminLog>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await parentBlockedByAdminLogService.SaveChangesLogAsync(
            parentId,
            userId,
            reason,
            isBlocked);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task SaveChangesLogAsync_WhenDataNotSavedToDb_ShouldReturn0()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var operationDate = DateTime.Now;
        var reason = "Reason to block parent";
        var isBlocked = true;

        parentBlockedByAdminLogRepositoryMock.Setup(x => x.Create(It.IsAny<ParentBlockedByAdminLog>()))
            .ReturnsAsync((ParentBlockedByAdminLog)null);

        // Act
        var result = await parentBlockedByAdminLogService.SaveChangesLogAsync(
            parentId,
            userId,
            reason,
            isBlocked);

        // Assert
        result.Should().Be(0);
    }

    #endregion
}