using System;
using System.Data.Common;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NUnit.Framework;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.WebApi.Config;

namespace OutOfSchool.WebApi.Tests.Common
{
    [TestFixture]
    public class MySqlConnectionExtensionTest
    {
        private readonly string connectionStringNoGuidFormat = @"{""ConnectionStrings"": {
        ""Test"": ""server=localhost;user=root;password=rootPassword;database=out_of_school""}}";

        private readonly string connectionString = @"{""ConnectionStrings"": {
        ""Test"": ""server=localhost;user=root;password=rootPassword;database=out_of_school;guidformat=binary16""}}";

        private readonly string overrides = @"{""ConnectionStringsOverride"": {
        ""Test"": {
            ""UseOverride"": true,
            ""Server"": ""localhost"",
            ""Port"": 3306,
            ""Database"": ""test"",
            ""UserId"": ""root"",
            ""Password"": ""rootPassword"",
            ""GuidFormat"": ""Binary16""
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
            ""GuidFormat"": ""Binary16""
        }
        },
        ""ConnectionStrings"": {
        ""Test"": ""server=localhost;user=root;password=rootPassword;database=out_of_school;guidformat=binary16""}}";

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
                    GuidFormat = options.GuidFormat.ToEnum(MySqlGuidFormat.Default),
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
            var ex = Assert.Throws<Exception>(() =>
                configuration.GetMySqlConnectionString<WebApiConnectionOptions>(
                    "Test",
                    options => new MySqlConnectionStringBuilder
                    {
                        Server = options.Server,
                        Port = options.Port,
                        UserID = options.UserId,
                        Password = options.Password,
                        Database = options.Database,
                        GuidFormat = options.GuidFormat.ToEnum(MySqlGuidFormat.Default),
                    }));
            Assert.AreEqual(
                ex?.Message,
                "The connection string should have a key: 'guidformat' and a value: 'binary16'");
        }

        [Test]
        public void IfNoOverrides_NoConnectionString_Throw()
        {
            // Arrange
            var configuration = Setup(connectionString);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() =>
                configuration.GetMySqlConnectionString<WebApiConnectionOptions>("Something"));
            Assert.AreEqual(ex?.Message, "Provide a valid connection string or options");
        }

        [Test]
        public void IfNoOverrides_ConnectionStringHasNoGuidFormat_Throw()
        {
            // Arrange
            var configuration = Setup(connectionStringNoGuidFormat);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() =>
                configuration.GetMySqlConnectionString<WebApiConnectionOptions>("Test"));
            Assert.AreEqual(
                ex?.Message,
                "The connection string should have a key: 'guidformat' and a value: 'binary16'");
        }

        private IConfiguration Setup(string json)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            return builder.Build();
        }
    }
}