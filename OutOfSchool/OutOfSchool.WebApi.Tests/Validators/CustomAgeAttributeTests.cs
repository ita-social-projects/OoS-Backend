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

        // Act
        var isValid = new CustomAgeAttribute().IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsNotDateTimeOrDateOnly_ShouldReturnFalse()
    {
        // Arrange
        var dateOfBirth = "1999-01-01";

        // Act
        var isValid = new CustomAgeAttribute().IsValid(dateOfBirth);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsDateTime_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow;

        // Act
        var isValid = new CustomAgeAttribute().IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsDateOnly_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var isValid = new CustomAgeAttribute().IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenMinAgeIsNotPositive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow;

        // Assert
        Assert.Throws<InvalidOperationException>(() => new CustomAgeAttribute() { MinAge = -1 }.IsValid(dateOfBirth));
    }

    [Test]
    public void IsValid_WhenMaxAgeIsNotPositive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow;

        // Assert
        Assert.Throws<InvalidOperationException>(() => new CustomAgeAttribute() { MaxAge = -1 }.IsValid(dateOfBirth));
    }

    [Test]
    public void IsValid_WhenMaxAgeIsLessThanMinAge_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow;

        // Assert
        Assert.Throws<InvalidOperationException>(() => new CustomAgeAttribute() { MaxAge = 1, MinAge = 2 }.IsValid(dateOfBirth));
    }

    [Test]
    public void IsValid_WhenDateIsLessThanMinAge_ShouldReturnFalse()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-1);

        // Act
        var isValid = new CustomAgeAttribute() { MinAge = 2 }.IsValid(dateOfBirth);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsGreaterThanMaxAge_ShouldReturnFalse()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-3);

        // Act
        var isValid = new CustomAgeAttribute() { MaxAge = 2 }.IsValid(dateOfBirth);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsExactlyMinAge_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-3);

        // Act
        var isValid = new CustomAgeAttribute() { MinAge = 3 }.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsExactlyMaxAge_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-3);

        // Act
        var isValid = new CustomAgeAttribute() { MaxAge = 3 }.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenDateIsBetweenMinAndMaxAge_ShouldReturnTrue()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-3);

        // Act
        var isValid = new CustomAgeAttribute() { MinAge = 2, MaxAge = 4 }.IsValid(dateOfBirth);

        // Assert
        Assert.IsTrue(isValid);
    }
}
