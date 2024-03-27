using NUnit.Framework;
using OutOfSchool.AuthCommon.Validators;

namespace OutOfSchool.AuthServer.Tests.Validators;

[TestFixture]
public class CustomPasswordRulesTests
{
    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("aaa", false)]
    [TestCase("AAAAAAAA", false)]
    [TestCase("Abbbbbbb", false)]
    [TestCase("Ab1bbbbb", false)]
    [TestCase("Ab1bbbb%", true)]
    public void IsValidPassword(string password, bool expectedResult)
    {
        // Act
        var result = CustomPasswordRules.IsValidPassword(password);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }
}
