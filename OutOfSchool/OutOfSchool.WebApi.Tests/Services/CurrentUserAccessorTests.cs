#nullable enable

using System.Collections.Generic;
using System.Security.Claims;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CurrentUserAccessorTests
{
    [Test]
    public void UserId_WhenUserIsNull_ReturnsEmptyString()
    {
        // Arrange
        ClaimsPrincipal? user = null;
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var userId = currentUser.UserId;

        // Assert
        Assert.AreEqual(string.Empty, userId);
    }

    [Test]
    public void UserId_WhenUserHasSubClaim_ReturnsSubClaimValue()
    {
        // Arrange
        var expected = "patron";
        var user = SetupClaimsPrincipal(IdentityResourceClaimsTypes.Sub, expected);
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var userId = currentUser.UserId;

        // Assert
        Assert.AreEqual(expected, userId);
    }

    [Test]
    public void UserRole_WhenUserIsNull_ReturnsEmptyString()
    {
        // Arrange
        ClaimsPrincipal? user = null;
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var userRole = currentUser.UserRole;

        // Assert
        Assert.AreEqual(string.Empty, userRole);
    }

    [Test]
    public void UserRole_WhenUserHasRoleClaim_ReturnsUserRole()
    {
        // Arrange
        var expected = "provider";
        var user = SetupClaimsPrincipal(IdentityResourceClaimsTypes.Role, expected);
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var userRole = currentUser.UserRole;

        // Assert
        Assert.AreEqual(expected, userRole);
    }

    [Test]
    public void IsInRole_WhenUserIsNull_ReturnsFalse()
    {
        // Arrange
        ClaimsPrincipal? user = null;
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var isInRole = currentUser.IsInRole("provider");

        // Assert
        Assert.IsFalse(isInRole);
    }

    [Test]
    public void IsInRole_WhenUserIsInRole_ReturnsTrue()
    {
        // Arrange
        var expected = "provider";
        var user = SetupClaimsPrincipal(ClaimTypes.Role, expected);
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var isInRole = currentUser.IsInRole(expected);

        // Assert
        Assert.IsTrue(isInRole);
    }

    [Test]
    public void IsInRole_WhenUserIsNotInRole_ReturnsFalse()
    {
        // Arrange
        var user = SetupClaimsPrincipal(ClaimTypes.Role, "parent");
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var isInRole = currentUser.IsInRole("provider");

        // Assert
        Assert.IsFalse(isInRole);
    }

    [Test]
    public void IsAuthenticated_WhenUserIsNull_ReturnsFalse()
    {
        // Arrange
        ClaimsPrincipal? user = null;
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var isAuthenticated = currentUser.IsAuthenticated;

        // Assert
        Assert.IsFalse(isAuthenticated);
    }

    [Test]
    public void IsAuthenticated_WhenUserIsAuthenticated_ReturnsTrue()
    {
        // Arrange
        var identity = new ClaimsIdentity(authenticationType: "test");
        var user = new ClaimsPrincipal(identity);
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var isAuthenticated = currentUser.IsAuthenticated;

        // Assert
        Assert.IsTrue(isAuthenticated);
    }

    [Test]
    public void IsAuthenticated_WhenUserIsNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var isAuthenticated = currentUser.IsAuthenticated;

        // Assert
        Assert.IsFalse(isAuthenticated);
    }

    [Test]
    public void HasClaim_WhenUserIsNull_ReturnsFalse()
    {
        // Arrange
        ClaimsPrincipal? user = null;
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var hasClaim = currentUser.HasClaim(ClaimTypes.Email);

        // Assert
        Assert.IsFalse(hasClaim);
    }

    [Test]
    public void HasClaim_WhenUserHasClaimType_ReturnsTrue()
    {
        // Arrange
        var user = SetupClaimsPrincipal(ClaimTypes.Email, "patron@gmail.com");
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var hasClaim = currentUser.HasClaim(ClaimTypes.Email);

        // Assert
        Assert.IsTrue(hasClaim);
    }

    [Test]
    public void HasClaim_WhenUserDoesNotHaveClaimType_ReturnsFalse()
    {
        // Arrange
        var user = SetupClaimsPrincipal(ClaimTypes.Name, "patron");
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var hasClaim = currentUser.HasClaim(ClaimTypes.Email);

        // Assert
        Assert.IsFalse(hasClaim);
    }

    [Test]
    public void HasClaim_WhenValueComparerMatches_ReturnsTrue()
    {
        // Arrange
        var expected = "patron@gmail.com";
        var user = SetupClaimsPrincipal(ClaimTypes.Email, "patron@gmail.com");
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var hasClaim = currentUser.HasClaim(ClaimTypes.Email, value => value == expected);

        // Assert
        Assert.IsTrue(hasClaim);
    }

    [Test]
    public void HasClaim_WhenValueComparerDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        var user = SetupClaimsPrincipal(ClaimTypes.Email, "patron@gmail.com");
        var currentUser = new CurrentUserAccessor(user);

        // Act
        var hasClaim = currentUser.HasClaim(ClaimTypes.Email, value => value == "mykola@gmail.com");

        // Assert
        Assert.IsFalse(hasClaim);
    }

    private static ClaimsPrincipal SetupClaimsPrincipal(string claimType, string expectedValue)
    {
        var claims = new List<Claim>
        {
            new(claimType, expectedValue),
        };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        return user;
    }
}