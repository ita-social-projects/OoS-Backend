using Microsoft.FeatureManagement;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static OutOfSchool.BusinessLogic.Validators.ConditionalValidationAttributes;

namespace OutOfSchool.WebApi.Tests.Validators;

[TestFixture]
public class ConditionalValidationAttributesTests
{

    #region ConditionalRequiredAttribute

    [Test]
    public void ConditionalRequiredAttributeIsValid_WhenEnabledFeature_ReturnsSuccess()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var attribute = new ConditionalRequiredAttribute(featureFlagName);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);

        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult("Some Value", validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void ConditionalRequiredAttributeIsValid_WhenStringIsEmpty_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var attribute = new ConditionalRequiredAttribute(featureFlagName);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);

        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(string.Empty, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual("This field is required when the feature is enabled.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalRequiredAttributeIsValid_WhenFeatureManagerIsNotRegistered_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var attribute = new ConditionalRequiredAttribute(featureFlagName);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult("Some Value", validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual("IFeatureManager service is not registered.", result.ErrorMessage);
    }

    #endregion

    #region ConditionalMinLengthAttribute

    [Test]
    public void ConditionalMinLengthAttributeIsValid_WhenFeatureManagerIsNotRegistered_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var minLength = 5;
        var attribute = new ConditionalMinLengthAttribute(featureFlagName, minLength);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult("Some Value", validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual("IFeatureManager service is not registered.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalMinLengthAttributeIsValid_WhenEnabledFeatureAndValueIsValid_ReturnsSuccess()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var minLength = 5;
        var attribute = new ConditionalMinLengthAttribute(featureFlagName, minLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult("Valid Value", validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void ConditionalMinLengthAttributeIsValid_WhenEnabledFeatureAndValueIsNull_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var minLength = 5;
        var attribute = new ConditionalMinLengthAttribute(featureFlagName, minLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(null, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual("This field is required when the feature is enabled.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalMinLengthAttributeIsValid_WhenEnabledFeatureAndValueIsEmptyString_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var minLength = 5;
        var attribute = new ConditionalMinLengthAttribute(featureFlagName, minLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(string.Empty, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual("This field is required when the feature is enabled.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalMinLengthAttributeIsValid_WhenEnabledFeatureAndStringValueIsTooShort_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var minLength = 5;
        var attribute = new ConditionalMinLengthAttribute(featureFlagName, minLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult("test", validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"The field must be at least {minLength} characters long when the feature is enabled.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalMinLengthAttributeIsValid_WhenEnabledFeatureAndCollecctionValueIsTooSmall_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var minLength = 5;
        var attribute = new ConditionalMinLengthAttribute(featureFlagName, minLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(new List<string> { "one", "two", "three" }, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"The collection must contain at least {minLength} items when the feature is enabled.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalMinLengthAttributeIsValid_WhenDisabledFeature_ReturnsSuccess()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var minLength = 5;
        var attribute = new ConditionalMinLengthAttribute(featureFlagName, minLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(false);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult("Valid Value", validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    #endregion

    #region ConditionalMaxLengthAttribute

    [Test]
    public void ConditionalMaxLengthAttributeIsValid_WhenFeatureManagerIsNotRegistered_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var maxLength = 5;
        var attribute = new ConditionalMaxLengthAttribute(featureFlagName, maxLength);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult("Some Value", validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual("IFeatureManager service is not registered.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalMaxLengthAttributeIsValid_WhenEnabledFeatureAndValueIsValid_ReturnsSuccess()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var maxLength = 11;
        string value = "Valid Value";
        var attribute = new ConditionalMaxLengthAttribute(featureFlagName, maxLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(value, validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void ConditionalMaxLengthAttributeIsValid_WhenEnabledFeatureAndValueIsNull_ReturnsSuccess()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var maxLength = 5;
        var attribute = new ConditionalMaxLengthAttribute(featureFlagName, maxLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(null, validationContext);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void ConditionalMaxLengthAttributeIsValid_WhenEnabledFeatureAndValueIsEmptyString_ReturnsSuccess()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var maxLength = 5;
        var attribute = new ConditionalMaxLengthAttribute(featureFlagName, maxLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(string.Empty, validationContext);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void ConditionalMaxLengthAttributeIsValid_WhenEnabledFeatureAndStringValueIsTooLong_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var maxLength = 10;
        string value = "Invalid Value";
        var attribute = new ConditionalMaxLengthAttribute(featureFlagName, maxLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(value, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"The field must be less than {maxLength + 1} characters long when the feature is enabled.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalMaxLengthAttributeIsValid_WhenEnabledFeatureAndCollecctionValueIsTooBig_ReturnsFalse()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var maxLength = 5;
        var value = new List<string> { "one", "two", "three", "four", "five", "six", "seven" };
        var attribute = new ConditionalMaxLengthAttribute(featureFlagName, maxLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(true);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(value, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"The collection must contain less than {maxLength + 1} items when the feature is enabled.", result.ErrorMessage);
    }

    [Test]
    public void ConditionalMaxLengthAttributeIsValid_WhenDisabledFeature_ReturnsSuccess()
    {
        // Arrange
        var featureFlagName = "TestFeature";
        var maxLength = 10;
        var value = "Invalid value";
        var attribute = new ConditionalMinLengthAttribute(featureFlagName, maxLength);
        var featureManager = new Mock<IFeatureManager>();
        var serviceProvider = new Mock<IServiceProvider>();

        featureManager.Setup(f => f.IsEnabledAsync(featureFlagName)).ReturnsAsync(false);
        serviceProvider.Setup(s => s.GetService(typeof(IFeatureManager))).Returns(featureManager.Object);
        var validationContext = new ValidationContext(instance: new object(), serviceProvider: serviceProvider.Object, items: null);

        // Act
        var result = attribute.GetValidationResult(value, validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    #endregion
}
