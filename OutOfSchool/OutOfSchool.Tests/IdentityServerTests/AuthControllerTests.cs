using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.IdentityServer.Controllers;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.IdentityServerTests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private readonly Mock<FakeUserManager> _mockUserManager;
        private readonly Mock<FakeSignInManager> _mockSignInManager;
        private readonly Mock<IIdentityServerInteractionService> _mockInteractionService;
        public AuthController CreateAuthController => new AuthController(_mockUserManager.Object, _mockSignInManager.Object, _mockInteractionService.Object);

        public AuthControllerTests()
        {
            _mockUserManager = new Mock<FakeUserManager>();
            _mockInteractionService = new Mock<IIdentityServerInteractionService>();
            _mockSignInManager = new Mock<FakeSignInManager>();
        }
        [SetUp]
        public void Setup()
        {
        }

        //[TestCase("", typeof(RedirectToActionResult))]
        //[TestCase("True logout id", typeof(RedirectResult))]
        [Test]
        public async Task LogoutWithLogoutId()
        {
            // Arrange
            var authController = CreateAuthController;
            var logoutRequest = new Mock<LogoutRequest>();
            logoutRequest.Setup(request => request.PostLogoutRedirectUri)
                .Returns("PostLogoutUrl");
            _mockSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            _mockInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest.Object));
            // Act
            var result =  await CreateAuthController.Logout("PostLogoutUrl");
            
            // Assert
            Assert.IsInstanceOf(typeof(RedirectResult), result);
        }   
    }
}