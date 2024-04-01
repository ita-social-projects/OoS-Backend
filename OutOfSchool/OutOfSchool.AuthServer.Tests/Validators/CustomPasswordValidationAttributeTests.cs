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

    [SetUp]
    public void SetUp()
    {
        attribute = new CustomPasswordValidationAttribute();
    }

    [Test]
    public void IsValid_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var validPassword = "Ab1%234C1";
        var validationContext = new ValidationContext(new object());

        // Act
        var result = attribute.GetValidationResult(validPassword, validationContext);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void IsValid_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var invalidPassword = "";
        var expectedErrorMessage = "Password must be at least 8 characters long";
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        var serviceProvider = new Mock<IServiceProvider>();
        localizer.Setup(x => x[Constants.PasswordValidationErrorMessage])
            .Returns(new LocalizedString("Error", expectedErrorMessage));
        serviceProvider.Setup(x => x.GetService(typeof(IStringLocalizer<SharedResource>)))
            .Returns(localizer.Object);
        var validationContext = new ValidationContext(invalidPassword, serviceProvider.Object, null);

        // Act
        var result = attribute.GetValidationResult(invalidPassword, validationContext);

        // Assert
        localizer.VerifyAll();
        serviceProvider.VerifyAll();
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedErrorMessage, result.ErrorMessage);
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
}