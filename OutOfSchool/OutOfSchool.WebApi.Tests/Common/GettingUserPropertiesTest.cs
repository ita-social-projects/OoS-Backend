using System.Security.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class GettingUserPropertiesTest
{
    private readonly Claim userIdClaim = new Claim(IdentityResourceClaimsTypes.Sub, "38776161-734b-4aec-96eb-4a1f87a2e5f3");
    private readonly Claim userRoleClaim = new Claim(IdentityResourceClaimsTypes.Role, "Parent");
    private readonly Claim userSubroleClaim = new Claim(IdentityResourceClaimsTypes.Subrole, "Provider");
    private Mock<HttpContext> httpContextMoq;

    [SetUp]
    public void Setup()
    {
        httpContextMoq = new Mock<HttpContext>();
        httpContextMoq.Setup(x => x.User.FindFirst("sub")).Returns(userIdClaim);
        httpContextMoq.Setup(x => x.User.FindFirst("role")).Returns(userRoleClaim);
        httpContextMoq.Setup(x => x.User.FindFirst("subrole")).Returns(userSubroleClaim);
    }

    [Test]
    public void GetUserId_ByHttpContext_ReturnsClaim()
    {
        // Assert
        Assert.AreEqual(userIdClaim.Value, GettingUserProperties.GetUserId(httpContextMoq.Object));
    }

    [Test]
    public void GetUserId_ByHttpContext_ThrowsAuthenticationException()
    {
        // Assert
        Assert.Throws<AuthenticationException>(
            () => GettingUserProperties.GetUserId((HttpContext)null));
    }

    [Test]
    public void GetUserId_ByClaimsPrincipal_ReturnNull()
    {
        // Assert
        Assert.IsNull(GettingUserProperties.GetUserId((ClaimsPrincipal)null));
    }

    [Test]
    public void GetUserRole_ByHttpContext_ReturnsClaim()
    {
        // Assert
        Assert.AreEqual(userRoleClaim.Value, GettingUserProperties.GetUserRole(httpContextMoq.Object).ToString());
    }

    [Test]
    public void GetUserRole_ByHttpContext_ThrowsAuthenticationException()
    {
        // Assert
        Assert.Throws<AuthenticationException>(
            () => GettingUserProperties.GetUserRole((HttpContext)null));
    }

    [Test]
    public void GetUserRole_ByClaimsPrincipal_ReturnNull()
    {
        // Assert
        Assert.IsNull(GettingUserProperties.GetUserRole((ClaimsPrincipal)null));
    }

    [Test]
    public void GetUserSubrole_ByHttpContext_ReturnsClaim()
    {
        // Assert
        Assert.AreEqual(userSubroleClaim.Value, GettingUserProperties.GetUserProviderSubRole(httpContextMoq.Object).ToString());
    }

    [Test]
    public void GetUserSubrole_ByHttpContext_ThrowsAuthenticationException()
    {
        // Assert
        Assert.Throws<AuthenticationException>(
            () => GettingUserProperties.GetUserProviderSubRole((HttpContext)null));
    }

    [Test]
    public void GetUserSubrole_ByClaimsPrincipal_ReturnNull()
    {
        // Assert
        Assert.IsNull(GettingUserProperties.GetUserProviderSubRole((ClaimsPrincipal)null));
    }
}