using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Validators;
using OutOfSchool.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthServer.Tests.Validators;

[TestFixture]
public class CustomPasswordValidationAttributeTests
{
    private CustomPasswordValidationAttribute attribute;
    private Mock<ICustomPasswordRules> rules;
    private Mock<IServiceProvider> serviceProvider;

    [SetUp]
    public void SetUp()
    {
        rules = new Mock<ICustomPasswordRules>();
        attribute = new CustomPasswordValidationAttribute();
        serviceProvider = new Mock<IServiceProvider>();
    }

    [Test]
    public void IsValid_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var validPassword = "Ab1%234C1";
        rules.Setup(x => x.IsValidPassword(validPassword)).Returns(true);
        serviceProvider.Setup(x => x.GetService(typeof(ICustomPasswordRules))).Returns(rules.Object);
        var validationContext = new ValidationContext(validPassword, serviceProvider.Object, null);

        // Act
        var result = attribute.GetValidationResult(validPassword, validationContext);

        // Assert
        serviceProvider.VerifyAll();
        rules.VerifyAll();
        Assert.IsNull(result);
    }

    [Test]
    public void IsValid_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var invalidPassword = "";
        var expectedErrorMessage = "Password must be at least 8 characters long";
        rules.Setup(x => x.IsValidPassword(invalidPassword)).Returns(false);
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        localizer.Setup(x => x[Constants.PasswordValidationErrorMessage])
            .Returns(new LocalizedString("Error", expectedErrorMessage));
        serviceProvider.Setup(x => x.GetService(typeof(IStringLocalizer<SharedResource>)))
            .Returns(localizer.Object);
        serviceProvider.Setup(x => x.GetService(typeof(ICustomPasswordRules))).Returns(rules.Object);
        var validationContext = new ValidationContext(invalidPassword, serviceProvider.Object, null);

        // Act
        var result = attribute.GetValidationResult(invalidPassword, validationContext);

        // Assert
        rules.VerifyAll();
        localizer.VerifyAll();
        serviceProvider.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedErrorMessage, result.ErrorMessage);
    }

    [Test]
    public void IsValid_WithInvalidPasswordAndLocalizerIsNotExist_ShouldReturnErrorName()
    {
        // Arrange
        var invalidPassword = "";
        var expectedErrorName = Constants.PasswordValidationErrorMessage;
        rules.Setup(x => x.IsValidPassword(invalidPassword)).Returns(false);
        serviceProvider.Setup(x => x.GetService(typeof(IStringLocalizer<SharedResource>)))
            .Returns(null);
        serviceProvider.Setup(x => x.GetService(typeof(ICustomPasswordRules))).Returns(rules.Object);
        var validationContext = new ValidationContext(invalidPassword, serviceProvider.Object, null);

        // Act
        var result = attribute.GetValidationResult(invalidPassword, validationContext);

        // Assert
        rules.VerifyAll();
        serviceProvider.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedErrorName, result.ErrorMessage);
    }

    [Test]
    public void IsValid_WithValidateContextIsNull_ThrowArgumentNullException()
    {
        // Arrange
        var password = "123";
        ValidationContext validationContext = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => attribute.GetValidationResult(password, validationContext));
    }

    [Test]
    public void IsValid_WithCustomPasswordRulesIsNotExist_ThrowNullReferenceException()
    {
        // Arrange
        var password = "123";
        var expectedExceptionMessage = "Value cannot be null. (Parameter 'ICustomPasswordRules')";
        serviceProvider.Setup(x => x.GetService(typeof(ICustomPasswordRules))).Returns(null);
        var validationContext = new ValidationContext(password, serviceProvider.Object, null);

        // Act & Assert
        Exception ex = Assert.Throws<ArgumentNullException>(() => attribute.GetValidationResult(password, validationContext));
        Assert.That(ex.Message, Is.EqualTo(expectedExceptionMessage));
    }
}