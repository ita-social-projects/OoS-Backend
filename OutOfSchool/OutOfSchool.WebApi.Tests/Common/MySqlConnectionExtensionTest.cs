using System;
using System.Data.Common;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Config.Quartz;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class MySqlConnectionExtensionTest
{
    private readonly string connectionStringNoGuidFormat = @"{""ConnectionStrings"": {
        ""Test"": ""server=localhost;user=root;password=rootPassword;database=out_of_school""}}";

    private readonly string connectionString = @"{""ConnectionStrings"": {
        ""Test"": ""server=localhost;user=root;password=rootPassword;database=out_of_school;oldguids=true""}}";

    private readonly string overrides = @"{""ConnectionStringsOverride"": {
        ""Test"": {
            ""UseOverride"": true,
            ""Server"": ""localhost"",
            ""Port"": 3306,
            ""Database"": ""test"",
            ""UserId"": ""root"",
            ""Password"": ""rootPassword"",
            ""OldGuids"": true
        }
        }}";

    private readonly string overridesNoGuidFormat = @"{""ConnectionStringsOverride"": {
        ""Test"": {
            ""UseOverride"": true,
            ""Server"": ""localhost"",
            ""Port"": 3306,
            ""Database"": ""test"",
            ""UserId"": ""root"",
            ""Password"": ""rootPassword""
        }
        }}";

    private readonly string overridesConnectionString = @"{""ConnectionStringsOverride"": {
        ""Test"": {
            ""UseOverride"": false,
            ""Server"": ""localhost"",
            ""Port"": 3306,
            ""Database"": ""test"",
            ""UserId"": ""root"",
            ""Password"": ""rootPassword"",
            ""OldGuids"": true
        }
        },
        ""ConnectionStrings"": {
        ""Test"": ""server=localhost;user=root;password=rootPassword;database=out_of_school;oldguids=true""}}";

    [Test]
    public void IfNoOverrides_UseConnectionString()
    {
        // Arrange
        var configuration = Setup(connectionString);

        // Act
        var connection = configuration.GetMySqlConnectionString<WebApiConnectionOptions>("Test");

        // Assert
        Assert.AreEqual(connection, configuration.GetConnectionString("Test"));
    }

    [Test]
    public void IfUseOverridesFalse_UseConnectionString()
    {
        // Arrange
        var configuration = Setup(overridesConnectionString);

        // Act
        var connection = configuration.GetMySqlConnectionString<WebApiConnectionOptions>("Test");

        // Assert
        Assert.AreEqual(connection, configuration.GetConnectionString("Test"));
    }

    [Test]
    public void IfUseOverridesTrue_CheckOptionsConverter()
    {
        // Arrange
        var configuration = Setup(overrides);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            configuration.GetMySqlConnectionString<WebApiConnectionOptions>("Test"));
    }

    [Test]
    public void IfUseOverridesTrue_Override()
    {
        // Arrange
        var configuration = Setup(overrides);

        // Act
        var connection = configuration.GetMySqlConnectionString<WebApiConnectionOptions>(
            "Test",
            options => new MySqlConnectionStringBuilder
            {
                Server = options.Server,
                Port = options.Port,
                UserID = options.UserId,
                Password = options.Password,
                Database = options.Database,
                OldGuids = true,
            });
        var builder = new DbConnectionStringBuilder()
        {
            ConnectionString = connection,
        };

        // Assert
        Assert.AreEqual("test", builder["database"]);
    }

    [Test]
    public void IfUseOverridesTrue_NoGuidFormat_Throw()
    {
        // Arrange
        var configuration = Setup(overridesNoGuidFormat);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            configuration.GetMySqlConnectionString<WebApiConnectionOptions>(
                "Test",
                options => new MySqlConnectionStringBuilder
                {
                    Server = options.Server,
                    Port = options.Port,
                    UserID = options.UserId,
                    Password = options.Password,
                    Database = options.Database,
                }));
        Assert.AreEqual(
            ex?.Message,
            "The connection string should have a key: 'oldguids' and a value: 'true'");
    }

    [Test]
    public void IfNoOverrides_NoConnectionString_Throw()
    {
        // Arrange
        var configuration = Setup(connectionString);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            configuration.GetMySqlConnectionString<WebApiConnectionOptions>("Something"));
        Assert.AreEqual(ex?.Message, "Provide a valid connection string or options");
    }

    [Test]
    public void IfNoOverrides_ConnectionStringHasNoGuidFormat_Throw()
    {
        // Arrange
        var configuration = Setup(connectionStringNoGuidFormat);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            configuration.GetMySqlConnectionString<WebApiConnectionOptions>("Test"));
        Assert.AreEqual(
            ex?.Message,
            "The connection string should have a key: 'oldguids' and a value: 'true'");
    }

    [Test]
    public void IfConnectionDoesNotRequireGuidFormat_GuidOptionIsRemoved()
    {
        // Arrange
        var configuration = Setup(connectionString);

        // Act
        var connection = configuration.GetMySqlConnectionString<QuartzConnectionOptions>("Test");

        // Assert
        Assert.False(connection.Contains("oldguids"));
    }

    [Test]
    public void IfConnectionDoesNotRequireGuidFormat_NoGuidFormatPresent_HandledWithoutError()
    {
        // Arrange
        var configuration = Setup(connectionStringNoGuidFormat);

        // Act
        var connection = configuration.GetMySqlConnectionString<QuartzConnectionOptions>("Test");

        // Assert
        Assert.False(connection.Contains("oldguids"));
    }

    private IConfiguration Setup(string json)
    {
        var builder = new ConfigurationBuilder();
        builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        return builder.Build();
    }
}