using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender;
using OutOfSchool.EmailSender.Services;
using Quartz;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class ServiceProviderExtensionsTests
{
    public static void ConfigureEmailOptions(OptionsBuilder<EmailOptions> builder)
    {
        builder.Configure(options =>
        {
            options.SendGridKey = string.Empty;
            options.NameFrom = "OOS_Test";
            options.AddressFrom = "oos-test@oos_test.ua";
            options.Enabled = true;
        });
    }

    [Test]
    public void AddEmailSenderService_WhenIsDevelopmentIsTrueAndSendGridApiKeyIsEmpty_DevEmailSenderServiceShouldRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var isDevelopment = true;
        var sendGridApikey = string.Empty;
        var loggerMock = new Mock<ILogger<DevEmailSender>>();
        services.AddSingleton(loggerMock.Object);

        // Act
        services.AddEmailSenderService(isDevelopment, sendGridApikey, builder =>
        {
            ConfigureEmailOptions(builder);
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOf<DevEmailSender>(provider.GetService<IEmailSenderService>());
    }

    [Test]
    public void AddEmailSenderService_WhenIsDevelopmentIsFalseAndSendGridApiKeyIsEmpty_EmailSenderServiceShouldRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var isDevelopment = false;
        var sendGridApikey = string.Empty;
        var loggerMock = new Mock<ILogger<DevEmailSender>>();
        var schedulerFactoryMock = new Mock<ISchedulerFactory>();
        services.AddSingleton(loggerMock.Object);
        services.AddSingleton(schedulerFactoryMock.Object);

        // Act
        services.AddEmailSenderService(isDevelopment, sendGridApikey, builder =>
        {
            ConfigureEmailOptions(builder);
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOf<EmailSenderService>(provider.GetService<IEmailSenderService>());
    }

    [Test]
    public void AddEmailSenderService_WhenIsNotDevelopmentAndEmailOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var isDevelopment = false;
        var sendGridApikey = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddEmailSenderService(isDevelopment, sendGridApikey, null));
    }
}
