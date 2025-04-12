using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ValidationServiceTests
{
    private Mock<IParentRepository> parentRepositoryMock;
    private Mock<IOfficialRepository> officialRepositoryMock;

    private IValidationService validationService;

    [SetUp]
    public void SetUp()
    {
        parentRepositoryMock = new Mock<IParentRepository>();
        officialRepositoryMock = new Mock<IOfficialRepository>();
        
        validationService = new ValidationService(
            parentRepositoryMock.Object,
            officialRepositoryMock.Object);
    }

    #region UserIsParentOwnerAsync
    [Test]
    public async Task UserIsParentOwnerAsync_WhenTrue_ReturnsTrue()
    {
        // Arrange
        var validUserId = "someUserId";
        var parentWithValidUserId = new Parent()
        {
            Id = Guid.NewGuid(),
            UserId = validUserId,
        };
        parentRepositoryMock
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(), 
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(new List<Parent>() { parentWithValidUserId });

        // Act
        var result = await validationService.UserIsParentOwnerAsync(validUserId, parentWithValidUserId.Id).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task UserIsParentOwnerAsync_WhenFalse_ReturnsFalse()
    {
        // Arrange
        var validUserId = "someUserId";
        var parentWithAnotherUserId = new Parent()
        {
            Id = Guid.NewGuid(),
            UserId = "anotherUserId",
        };
        parentRepositoryMock
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(), 
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(new List<Parent>() { parentWithAnotherUserId });

        // Act
        var result = await validationService.UserIsParentOwnerAsync(validUserId, parentWithAnotherUserId.Id).ConfigureAwait(false);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task UserIsParentOwnerAsync_WhenEntityWasNotFound_ReturnsFalse()
    {
        // Arrange
        var validUserId = "someUserId";
        parentRepositoryMock
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(new List<Parent>());

        // Act
        var result = await validationService.UserIsParentOwnerAsync(validUserId, Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region GetParentOrProviderIdByUserRoleAsync
    [Test]
    public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsParentAndParentExists_ReturnsId()
    {
        // Arrange
        var validUserId = "someUserId";
        var userRole = Role.Parent;

        var parentWithValidUserId = new Parent()
        {
            Id = Guid.NewGuid(),
            UserId = validUserId,
        };
        parentRepositoryMock
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(), 
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(new List<Parent>() { parentWithValidUserId });

        // Act
        var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(parentWithValidUserId.Id, result);
    }

    [Test]
    public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsParentAndParentDoesNotExist_ReturnsZero()
    {
        // Arrange
        var validUserId = "someUserId";
        var userRole = Role.Parent;

        parentRepositoryMock
            .Setup(x => x.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(), 
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(new List<Parent>() { });

        // Act
        var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(default(Guid), result);
    }

    [Test]
    public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsProviderAndProviderExists_ReturnsId()
    {
        // Arrange
        var validUserId = "someUserId";
        var userRole = Role.Provider;

        var providerWithValidUserId = new Provider()
        {
            Id = Guid.NewGuid(),
        };
        
        officialRepositoryMock.Setup(s => s.GetProviderIdByOfficialUserIdAsync(validUserId))
            .ReturnsAsync(providerWithValidUserId.Id);

        // Act
        var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(providerWithValidUserId.Id, result);
    }

    [Test]
    public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsProviderAndProviderDoesNotExist_ReturnsZero()
    {
        // Arrange
        var validUserId = "someUserId";
        var userRole = Role.Provider;
        officialRepositoryMock.Setup(s => s.GetProviderIdByOfficialUserIdAsync(validUserId))
            .ReturnsAsync(Guid.Empty);

        // Act
        var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(default(Guid), result);
    }

    [Test]
    public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsNotParentNotProvider_ReturnsZero()
    {
        // Arrange
        var validUserId = "someUserId";
        var userRole = Role.TechAdmin;

        // Act
        var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(default(Guid), result);
    }
    #endregion
}