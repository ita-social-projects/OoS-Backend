using NUnit.Framework;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Tests.Validators;

[TestFixture]
public class MustContainAttributeTests
{

    [Test]
    [TestCase("123", RequiredCharacterType.Digit)]
    public void MustContainDigit_ValidInput_ReturnsSuccess(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    [TestCase("abc", RequiredCharacterType.Digit)]
    public void MustContainDigit_InvalidInput_ReturnsError(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.IsNotNull(result);
        Assert.AreEqual("Field must contain at least one digit.", result.ErrorMessage);
    }

    [Test]
    [TestCase("abc", RequiredCharacterType.LatinLetter)]
    public void MustContainLatinLetter_ValidInput_ReturnsSuccess(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    [TestCase("123", RequiredCharacterType.LatinLetter)]
    public void MustContainLatinLetter_InvalidInput_ReturnsError(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.IsNotNull(result);
        Assert.AreEqual("Field must contain at least one latin letter.", result.ErrorMessage);
    }

    [Test]
    [TestCase("абв", RequiredCharacterType.CyrillicLetter)]
    public void MustContainCyrillicLetter_ValidInput_ReturnsSuccess(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    [TestCase("abc", RequiredCharacterType.CyrillicLetter)]
    public void MustContainCyrillicLetter_InvalidInput_ReturnsError(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.IsNotNull(result);
        Assert.AreEqual("Field must contain at least one cyrillic letter.", result.ErrorMessage);
    }

    [Test]
    [TestCase("abc", RequiredCharacterType.AnyLetter)]
    public void MustContainAnyLetter_ValidInput_ReturnsSuccess(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    [TestCase("123", RequiredCharacterType.AnyLetter)]
    public void MustContainAnyLetter_InvalidInput_ReturnsError(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.IsNotNull(result);
        Assert.AreEqual("Field must contain at least one letter.", result.ErrorMessage);
    }

    [Test]
    [TestCase("", RequiredCharacterType.Digit)]
    [TestCase(null, RequiredCharacterType.Digit)]
    public void MustContainAttribute_EmptyOrNullInput_ReturnsSuccess(string input, RequiredCharacterType type)
    {
        var attribute = new MustContainAttribute(type);
        var result = attribute.GetValidationResult(input, new ValidationContext(new object()));
        Assert.AreEqual(ValidationResult.Success, result);
    }
}
