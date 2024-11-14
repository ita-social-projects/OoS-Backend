using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender;
using OutOfSchool.EmailSender.Senders;
using OutOfSchool.EmailSender.Services;
using Quartz;
using SendGrid;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class ServiceProviderExtensionsTests
{
    private ServiceCollection _services;
    private Mock<ISchedulerFactory> _schedulerFactoryMock;
    private Mock<ISendGridAccessibilityService> _sendGridAccessibilityService;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
        _schedulerFactoryMock = new Mock<ISchedulerFactory>();
        _sendGridAccessibilityService = new Mock<ISendGridAccessibilityService>();
        _services.AddSingleton(_schedulerFactoryMock.Object);
        _services.AddSingleton(_sendGridAccessibilityService.Object);
    }

    [Test]
    public void AddEmailSenderService_ShouldRegisterIEmailSenderServiceAsTransient()
    {
        // Arrange
        Action<OptionsBuilder<EmailOptions>> emailOptions = options => {};

        // Act
        _services.AddEmailSenderService(emailOptions);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var service1 = serviceProvider.GetService<IEmailSenderService>();
        var service2 = serviceProvider.GetService<IEmailSenderService>();
        Assert.IsNotNull(service1);
        Assert.AreNotSame(service1, service2);
    }

    [Test]
    public void AddEmailSenderService_WhenSendGridApiKeyIsEmpty_EmailSenderServiceShouldRegister()
    {
        // Act
        _services.AddEmailSenderService(ConfigureEmailOptions);
        ServiceProvider provider = _services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOf<EmailSenderService>(provider.GetService<IEmailSenderService>());
    }

    [Test]
    public void AddEmailSenderService_WhenEmailOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddEmailSenderService(null));
    }

    [Test]
    public void AddEmailSenderService_WhenEmailOptionsIsNotNull_AppliesEmailOptionsConfiguration()
    {
        // Arrange
        var mockEmailOptions = new Mock<Action<OptionsBuilder<EmailOptions>>>();

        // Act
        _services.AddEmailSenderService(mockEmailOptions.Object);
        _services.BuildServiceProvider();

        // Assert
        mockEmailOptions.Verify(
            opt => opt(It.IsAny<OptionsBuilder<EmailOptions>>()),
            Times.Once);
    }

    [Test]
    public void AddEmailSender_DevEmailSenderServiceShouldRegister()
    {
        // Arrange
        const bool isDevelopment = true;
        string sendGridApikey = string.Empty;
        _services.AddSingleton(new Mock<ILogger<DevelopmentEmailSender>>().Object);

        // Act
        _services.AddEmailSender(isDevelopment, sendGridApikey);
        ServiceProvider provider = _services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOf<DevelopmentEmailSender>(provider.GetService<IEmailSender>());
    }

    [Test]
    public void AddEmailSender_RegistersSendGridEmailSender_WhenInProductionAndApiKeyIsValid()
    {
        // Arrange
        const bool isDevelopment = false;
        const string sendGridApiKey = "ValidApiKey";

        // Act
        _services.AddEmailSender(isDevelopment, sendGridApiKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var emailSender = serviceProvider.GetService<IEmailSender>();
        Assert.IsNotNull(emailSender);
        Assert.IsInstanceOf<SendGridEmailSender>(emailSender);
    }

    [Test]
    public void AddEmailSender_RegistersSendGridEmailSenderWithCorrectOptions_WhenInProductionAndApiKeyIsValid()
    {
        // Arrange
        const bool isDevelopment = false;
        const string sendGridApiKey = "ValidApiKey";

        // Act
        _services.AddEmailSender(isDevelopment, sendGridApiKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<IOptions<SendGridClientOptions>>();
        Assert.IsNotNull(options);
        Assert.AreEqual(sendGridApiKey, options.Value.ApiKey);
        Assert.IsTrue(options.Value.HttpErrorAsException);
    }

    [Test]
    public void AddEmailSender_AssignsPlaceholderApiKey_WhenInProductionAndApiKeyIsEmpty()
    {
        // Arrange
        const bool isDevelopment = false;
        string sendGridApiKey = string.Empty;
        const string PlaceholderForSendGridApiKey = "x";

        // Act
        _services.AddEmailSender(isDevelopment, sendGridApiKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<IOptions<SendGridClientOptions>>();
        Assert.IsNotNull(options);
        Assert.AreEqual(PlaceholderForSendGridApiKey, options.Value.ApiKey);
        Assert.IsTrue(options.Value.HttpErrorAsException);
    }

    private static void ConfigureEmailOptions(OptionsBuilder<EmailOptions> builder)
    {
        builder.Configure(options =>
        {
            options.SendGridKey = string.Empty;
            options.NameFrom = "OOS_Test";
            options.AddressFrom = "oos-test@oos_test.ua";
            options.Enabled = true;
        });
    }
}