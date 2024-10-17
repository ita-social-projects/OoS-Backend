using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BackgroundJobs.Extensions.Startup;
using Quartz;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Extensions.Startup;

[TestFixture]
public class QuartzExtensionTests
{
    [Test]
    public void AddDefaultQuartz_WhenServiceCollectionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var servicesRegistering = null as IServiceCollection;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => servicesRegistering.AddDefaultQuartz(It.IsAny<IConfiguration>()));
    }

    [Test]
    public void AddDefaultQuartz_WhenConfigurationIsOk_ShouldRegisterQuartz()
    {
        // Arrange
        var servicesRegistering = new ServiceCollection();

        servicesRegistering.AddSingleton<ILoggerFactory>(new LoggerFactory());

        var configDict = new Dictionary<string, string>()
        {
            { "ConnectionStrings:QuartzConnection", "server=test;user=test;password=test;database=test" },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        // Act
        servicesRegistering.AddDefaultQuartz(configuration);

        // Assert
        Assert.IsTrue(servicesRegistering.Any(x => x.ServiceType == typeof(ISchedulerFactory)));
    }
}
