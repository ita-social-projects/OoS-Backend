using System;
using NUnit.Framework;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.WebApi.Tests.Validators;

[TestFixture]
public class CustomAgeAttributeTests
{
    [Test]
    public void IsValid_WhenDateIsNull_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = null as DateTime?;
        var customDayOfBirthAttribute = new CustomAgeAttribute();

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsNotDateTimeOrDateOnly_ShouldReturnFalse()
    {
        // Arrange
        var dateOfBirth = "1999-01-01";
        var customDayOfBirthAttribute = new CustomAgeAttribute();

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsDateTime_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow;
        var customDayOfBirthAttribute = new CustomAgeAttribute();

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsDateOnly_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow);
        var customDayOfBirthAttribute = new CustomAgeAttribute();

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenMinAgeIsNotPositive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow;
        var customDayOfBirthAttribute = new CustomAgeAttribute() { MinAge = -1 };

        // Act
        TestDelegate action = () => customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Test]
    public void IsValid_WhenMaxAgeIsNotPositive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow;
        var customDayOfBirthAttribute = new CustomAgeAttribute() { MaxAge = -1 };

        // Act
        TestDelegate action = () => customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Test]
    public void IsValid_WhenMaxAgeIsLessThanMinAge_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow;
        var customDayOfBirthAttribute = new CustomAgeAttribute() { MaxAge = 1, MinAge = 2 };

        // Act
        TestDelegate action = () => customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Test]
    public void IsValid_WhenDateIsLessThanMinAge_ShouldReturnFalse()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-1);
        var customDayOfBirthAttribute = new CustomAgeAttribute() { MinAge = 2 };

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsGreaterThanMaxAge_ShouldReturnFalse()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-3);
        var customDayOfBirthAttribute = new CustomAgeAttribute() { MaxAge = 2 };

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsExactlyMinAge_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-3);
        var customDayOfBirthAttribute = new CustomAgeAttribute() { MinAge = 3 };

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsExactlyMaxAge_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-3);
        var customDayOfBirthAttribute = new CustomAgeAttribute() { MaxAge = 3 };

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsBetweenMinAndMaxAge_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-3);
        var customDayOfBirthAttribute = new CustomAgeAttribute() { MinAge = 2, MaxAge = 4 };

        // Act
        var isValid = customDayOfBirthAttribute.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }
}
