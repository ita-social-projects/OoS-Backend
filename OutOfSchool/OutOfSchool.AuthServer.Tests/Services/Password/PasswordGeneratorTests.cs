using OutOfSchool.Common;
using NUnit.Framework;
using System.Linq;
using OutOfSchool.AuthCommon.Services.Password;

namespace OutOfSchool.AuthServer.Tests.Services.Password;

[TestFixture]
public class PasswordGeneratorTests
{
    [Test]
    public void PasswordGenerator_ShouldReturnValidPassword()
    {
        // Act
        var result = PasswordGenerator.GenerateRandomPassword();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any(char.IsUpper));
        Assert.IsTrue(result.Any(char.IsLower));
        Assert.IsTrue(result.Any(char.IsDigit));
        Assert.IsTrue(result.Any(Constants.ValidationSymbols.Contains));
        Assert.IsFalse(result.Any(c => !char.IsLetterOrDigit(c) && !Constants.ValidationSymbols.Contains(c)),
            "Password shouldn't contain any other symbols than digits, upper/lowercase letters, and allowed special symbols");
        Assert.AreEqual(Constants.PasswordMinLength, result.Length);
    }
}
