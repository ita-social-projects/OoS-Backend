using System;
using NUnit.Framework;
using OutOfSchool.Common.Extensions;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class StringExtensionsTest
{
    private readonly string enumNull = null;
    private readonly string enumEmpty = string.Empty;
    private readonly string enumString = "Example";
    private readonly string notEnumString = "Hello";

    [Test]
    public void IfNull_ReturnDefault()
    {
        var result = enumNull.ToEnum(Test.Default);
        Assert.AreEqual(Test.Default, result);
    }

    [Test]
    public void IfEmpty_ReturnDefault()
    {
        var result = enumEmpty.ToEnum(Test.Default);
        Assert.AreEqual(Test.Default, result);
    }

    [Test]
    public void IfNotExistValue_ReturnDefault()
    {
        var result = notEnumString.ToEnum(Test.Default);
        Assert.AreEqual(Test.Default, result);
    }

    [Test]
    public void IfCorrectValue_ReturnParsedEnum()
    {
        var result = enumString.ToEnum(Test.Default);
        Assert.AreEqual(Test.Example, result);
    }

    [TestCase("abcde12345", 1, "a")]
    [TestCase("abcde12345", 5, "abcde")]
    [TestCase("abcde12345", 100, "abcde12345")]
    [TestCase("abcde12345", 0, "abcde12345")]
    [TestCase(null, 100, null)]
    public void Limit(string initialValue, int maxLength, string expectedResult)
    {
        var result = initialValue.Limit(maxLength);
        Assert.AreEqual(expectedResult, result);
    }

    [Test]
    public void Limit_MaxLength_LessThanZero_ThrowsException()
    {
        var initialValue = "abcde12345";

        Assert.Throws(typeof(ArgumentOutOfRangeException), () => initialValue.Limit(-1));
    }

    internal enum Test
    {
        Default,
        Example,
    }
}