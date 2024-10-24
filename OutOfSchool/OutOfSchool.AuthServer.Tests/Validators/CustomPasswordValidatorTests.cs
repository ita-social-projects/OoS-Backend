﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Validators;
using OutOfSchool.Common;
using OutOfSchool.Services.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.AuthServer.Tests.Validators;

[TestFixture]
public class CustomPasswordValidatorTests
{
    private IPasswordValidator<User> customPasswordValidator;
    private Mock<UserManager<User>> userManager;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<ICustomPasswordRules> rules;

    [SetUp]
    public void SetUp()
    {
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        rules = new Mock<ICustomPasswordRules>();

        userManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                Array.Empty<IUserValidator<User>>(),
                Array.Empty<IPasswordValidator<User>>(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

        customPasswordValidator = new CustomPasswordValidator(localizer.Object, rules.Object);
    }

    [Test]
    public async Task ValidateAsync_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var user = new User();
        var validPassword = "A1b2c4%d";
        rules.Setup(x => x.IsValidPassword(validPassword)).Returns(true);

        // Act
        var result = await customPasswordValidator.ValidateAsync(userManager.Object, user, validPassword);

        // Assert
        Assert.IsTrue(result.Succeeded);
    }

    [Test]
    public async Task ValidateAsync_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var user = new User();
        var invalidPassword = "";
        rules.Setup(x => x.IsValidPassword(invalidPassword)).Returns(false);
        var expectedErrorMessageName = "PasswordValidationErrorMessage";
        var expectedErrorMessage = "Password is invalid";
        localizer.Setup(x => x[Constants.PasswordValidationErrorMessage])
            .Returns(new LocalizedString(expectedErrorMessageName, expectedErrorMessage));

        // Act
        var result = await customPasswordValidator.ValidateAsync(userManager.Object, user, invalidPassword);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(expectedErrorMessage, result.Errors.First().Description);
    }
}
