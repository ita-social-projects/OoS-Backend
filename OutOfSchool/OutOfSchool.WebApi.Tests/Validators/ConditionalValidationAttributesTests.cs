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
        var result = attribute.GetValidationResult(new List<string> { "one", "two", "three"}, validationContext);

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
}
