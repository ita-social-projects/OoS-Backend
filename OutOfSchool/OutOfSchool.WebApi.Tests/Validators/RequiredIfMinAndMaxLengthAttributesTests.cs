using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using static OutOfSchool.BusinessLogic.Validators.RequiredIfMinAndMaxLengthAttributes;

namespace OutOfSchool.WebApi.Tests.Validators;

public class RequiredIfMinAndMaxLengthAttributesTests
{
    #region RequiredIfMinLengthAttributesTests

    [Test]
    public void RequiredIfMinLengthAttribute_WhenBoolPropertyIsTrueAndValueIsTooShort_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        var dependentPropertyValue = "Some Value";
        var minLength = 15;
        var attribute = new RequiredIfMinLengthAttribute("BoolProperty", boolPropertyValue, minLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"The field must be at least {minLength} characters long when BoolProperty = {boolPropertyValue}.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMinLengthAttribute_WhenBoolPropertyIsTrueAndValueIsValid_ReturnsSuccess()
    {
        // Arrange
        var boolPropertyValue = true;
        var dependentPropertyValue = "Some Value";
        var minLength = 5;
        var attribute = new RequiredIfMinLengthAttribute("BoolProperty", boolPropertyValue, minLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void RequiredIfMinLengthAttribute_WhenBoolPropertyIsTrueAndValueIsNull_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        string dependentPropertyValue = null;
        var minLength = 5;
        var attribute = new RequiredIfMinLengthAttribute("BoolProperty", boolPropertyValue, minLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"This field is required when BoolProperty = {boolPropertyValue}.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMinLengthAttribute_WhenBoolPropertyIsTrueAndValueIsEmptyString_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        string dependentPropertyValue = string.Empty;
        var minLength = 5;
        var attribute = new RequiredIfMinLengthAttribute("BoolProperty", boolPropertyValue, minLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"This field is required when BoolProperty = {boolPropertyValue}.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMinLengthAttribute_WhenBoolPropertyIsFalseAndValueIsTooShort_ReturnsSuccess()
    {
        // Arrange
        var boolPropertyValue = false;
        var dependentPropertyValue = "Some Value";
        var minLength = 15;
        var attribute = new RequiredIfMinLengthAttribute("BoolProperty", boolPropertyValue, minLength);
        var model = new TestModel { BoolProperty = !boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void RequiredIfMinLengthAttribute_WhenPropertyIsNotFound_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        string dependentPropertyValue = string.Empty;
        var minLength = 5;
        var attribute = new RequiredIfMinLengthAttribute("IncorrectBoolPropertyName", boolPropertyValue, minLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual("Property IncorrectBoolPropertyName not found.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMinLengthAttribute_WhenBoolPropertyIsTrueAndCollectionIsTooShort_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        var dependentPropertyValue = new List<string> { "Some Value1", "Some Value2", "Some Value3" };
        var minLength = 15;
        var attribute = new RequiredIfMinLengthAttribute("BoolProperty", boolPropertyValue, minLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, CollectionDependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"The collection must contain at least {minLength} items when BoolProperty = {boolPropertyValue}.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMinLengthAttribute_WhenBoolPropertyIsTrueAndCollectionIsValid_ReturnsSuccess()
    {
        // Arrange
        var boolPropertyValue = true;
        var dependentPropertyValue = new List<string> { "Some Value1", "Some Value2", "Some Value3" };
        var minLength = 2;
        var attribute = new RequiredIfMinLengthAttribute("BoolProperty", boolPropertyValue, minLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, CollectionDependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    #endregion

    #region RequiredIfMaxLengthAttribute

    [Test]
    public void RequiredIfMaxLengthAttribute_WhenBoolPropertyIsTrueAndValueIsTooLarge_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        var dependentPropertyValue = "Some Value";
        var maxLength = 5;
        var attribute = new RequiredIfMaxLengthAttribute("BoolProperty", boolPropertyValue, maxLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"The field must not be greater than {maxLength} characters long when BoolProperty = {boolPropertyValue}.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMaxLengthAttribute_WhenBoolPropertyIsTrueAndValueIsValid_ReturnsSuccess()
    {
        // Arrange
        var boolPropertyValue = true;
        var dependentPropertyValue = "Some Value";
        var maxLength = 15;
        var attribute = new RequiredIfMaxLengthAttribute("BoolProperty", boolPropertyValue, maxLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void RequiredIfMaxLengthAttribute_WhenBoolPropertyIsTrueAndValueIsNull_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        string dependentPropertyValue = null;
        var maxLength = 5;
        var attribute = new RequiredIfMaxLengthAttribute("BoolProperty", boolPropertyValue, maxLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"This field is required when BoolProperty = {boolPropertyValue}.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMaxLengthAttribute_WhenBoolPropertyIsTrueAndValueIsEmptyString_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        string dependentPropertyValue = string.Empty;
        var maxLength = 5;
        var attribute = new RequiredIfMaxLengthAttribute("BoolProperty", boolPropertyValue, maxLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"This field is required when BoolProperty = {boolPropertyValue}.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMaxLengthAttribute_WhenBoolPropertyIsFalseAndValueIsTooLarge_ReturnsSuccess()
    {
        // Arrange
        var boolPropertyValue = false;
        var dependentPropertyValue = "Some Value";
        var maxLength = 5;
        var attribute = new RequiredIfMaxLengthAttribute("BoolProperty", boolPropertyValue, maxLength);
        var model = new TestModel { BoolProperty = !boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void RequiredIfMaxLengthAttribute_WhenPropertyIsNotFound_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        string dependentPropertyValue = string.Empty;
        var maxLength = 5;
        var attribute = new RequiredIfMaxLengthAttribute("IncorrectBoolPropertyName", boolPropertyValue, maxLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, DependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual("Property IncorrectBoolPropertyName not found.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMaxLengthAttribute_WhenBoolPropertyIsTrueAndCollectionIsTooLarge_ReturnsValidationError()
    {
        // Arrange
        var boolPropertyValue = true;
        var dependentPropertyValue = new List<string> { "Some Value1", "Some Value2", "Some Value3" };
        var maxLength = 2;
        var attribute = new RequiredIfMaxLengthAttribute("BoolProperty", boolPropertyValue, maxLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, CollectionDependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ValidationResult>(result);
        Assert.AreEqual($"The collection must not contain greater than {maxLength} items when BoolProperty = {boolPropertyValue}.", result.ErrorMessage);
    }

    [Test]
    public void RequiredIfMaxLengthAttribute_WhenBoolPropertyIsTrueAndCollectionIsValid_ReturnsSuccess()
    {
        // Arrange
        var boolPropertyValue = true;
        var dependentPropertyValue = new List<string> { "Some Value1", "Some Value2", "Some Value3" };
        var maxLength = 5;
        var attribute = new RequiredIfMaxLengthAttribute("BoolProperty", boolPropertyValue, maxLength);
        var model = new TestModel { BoolProperty = boolPropertyValue, CollectionDependentProperty = dependentPropertyValue };
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);

        // Act
        var result = attribute.GetValidationResult(dependentPropertyValue, validationContext);

        // Assert
        Assert.AreEqual(ValidationResult.Success, result);
    }

    #endregion

    public class TestModel
    {
        public bool BoolProperty { get; set; }
        public string DependentProperty { get; set; }
        public List<string> CollectionDependentProperty { get; set; }
    }
}