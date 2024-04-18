using NUnit.Framework;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.WebApi.Tests.Validators;

[TestFixture]
public class CustomPhoneNumberAttributeTests
{
    [Test]
    public void IsValid_WhenPhoneNumberIsNull_ShouldReturnTrue()
    {
        // Arrange
        var phoneNumber = null as string;
        var customPhoneNumberAttribute = new CustomPhoneNumberAttribute();

        // Act
        var isValid = customPhoneNumberAttribute.IsValid(phoneNumber);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenPhoneNumberIsNotString_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = 111111111;
        var customPhoneNumberAttribute = new CustomPhoneNumberAttribute();

        // Act
        var isValid = customPhoneNumberAttribute.IsValid(phoneNumber);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenPhoneNumberIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = string.Empty;
        var customPhoneNumberAttribute = new CustomPhoneNumberAttribute();

        // Act
        var isValid = customPhoneNumberAttribute.IsValid(phoneNumber);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenPhoneNumberIsNotStartsWithPlus_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = "1111111";
        var customPhoneNumberAttribute = new CustomPhoneNumberAttribute();

        // Act
        var isValid = customPhoneNumberAttribute.IsValid(phoneNumber);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenPhoneNumberIsShort_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = "+111111";
        var customPhoneNumberAttribute = new CustomPhoneNumberAttribute();

        // Act
        var isValid = customPhoneNumberAttribute.IsValid(phoneNumber);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenPhoneNumberIsLong_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = "+1111111111111111";
        var customPhoneNumberAttribute = new CustomPhoneNumberAttribute();

        // Act
        var isValid = customPhoneNumberAttribute.IsValid(phoneNumber);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenPhoneNumberIsContainsNotDigits_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = "+111111abc11";
        var customPhoneNumberAttribute = new CustomPhoneNumberAttribute();

        // Act
        var isValid = customPhoneNumberAttribute.IsValid(phoneNumber);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenPhoneNumberIsValid_ShouldReturnTrue()
    {
        // Arrange
        var phoneNumber = "+11111111111";
        var customPhoneNumberAttribute = new CustomPhoneNumberAttribute();

        // Act
        var isValid = customPhoneNumberAttribute.IsValid(phoneNumber);

        // Assert
        Assert.IsTrue(isValid);
    }
}
