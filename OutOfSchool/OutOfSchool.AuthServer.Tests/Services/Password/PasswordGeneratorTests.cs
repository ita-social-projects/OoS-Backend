using NUnit.Framework;
using OutOfSchool.AuthCommon.Services.Password;
using System.Linq;

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
        Assert.That(result, Does.Match(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8}$"));
    }
}
