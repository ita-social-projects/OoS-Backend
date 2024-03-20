using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender;

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
    public void AddEmailSender_WhenIsDevelopmentIsTrueAndSendGridApiKeyIsEmpty_DevEmailSenderServiceShouldRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var isDevelopment = true;
        var sendGridApikey = string.Empty;
        var loggerMock = new Mock<ILogger<DevEmailSender>>();
        services.AddSingleton(loggerMock.Object);

        // Act
        services.AddEmailSender(isDevelopment, sendGridApikey, builder =>
        {
            ConfigureEmailOptions(builder);
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOf<DevEmailSender>(provider.GetService<IEmailSenderService>());
    }

    [Test]
    public void AddEmailSender_WhenIsDevelopmentIsFalseAndSendGridApiKeyIsEmpty_EmailSenderServiceShouldRegister()
    {
        // Arrange
        var services = new ServiceCollection();
        var isDevelopment = false;
        var sendGridApikey = string.Empty;
        var loggerMock = new Mock<ILogger<DevEmailSender>>();
        services.AddSingleton(loggerMock.Object);

        // Act
        services.AddEmailSender(isDevelopment, sendGridApikey, builder =>
        {
            ConfigureEmailOptions(builder);
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOf<EmailSender.EmailSenderService>(provider.GetService<IEmailSenderService>());
    }

    [Test]
    public void AddEmailSender_WhenIsNotDevelopmentAndEmailOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var isDevelopment = false;
        var sendGridApikey = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddEmailSender(isDevelopment, sendGridApikey, null));
    }
}
