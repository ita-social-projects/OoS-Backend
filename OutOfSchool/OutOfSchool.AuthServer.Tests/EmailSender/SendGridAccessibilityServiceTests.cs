﻿using NUnit.Framework;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.EmailSender;
using System;
using Microsoft.Extensions.Options;

namespace OutOfSchool.AuthServer.Tests.EmailSender;

[TestFixture]
public class SendGridAccessibilityServiceTests
{
    private IOptions<EmailOptions> _emailOptions;
    private ISendGridAccessibilityService _emailService;

    [SetUp]
    public void Setup()
    { 
        _emailOptions = Options.Create(new EmailOptions { TimeoutTime = 300 });
        _emailService = new SendGridAccessibilityService(_emailOptions);
    }

    [Test]
    public void IsSendGridAccessible_ReturnsTrueWhenAccessible()
    {
        // Arrange
        var service = new SendGridAccessibilityService(_emailOptions);
        var now = DateTimeOffset.Now;

        // Act
        var result = service.IsSendGridAccessible(now);

        // Assert
        Assert.True(result);
    }

    [Test]
    public void IsSendGridAccessible_ReturnsFalseWhenNotAccessible()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        // Act
        _emailService.SetSendGridInaccessible(now);

        var result = _emailService.IsSendGridAccessible(now);

        // Assert
        Assert.False(result);
    }

    [Test]
    public void IsSendGridAccessible_WithOptionsTimeoutTimeNotNull_ReturnsTrueAfterTimeoutTimePassed()
    {
        // Arrange
        var now = DateTimeOffset.Now;

        // Act
        _emailService.SetSendGridInaccessible(now);

        var earlyResult = _emailService.IsSendGridAccessible(now + TimeSpan.FromMinutes(250));
        var result = _emailService.IsSendGridAccessible(now + TimeSpan.FromMinutes(301));

        // Assert
        Assert.False(earlyResult);
        Assert.True(result);
    }

    [Test]
    public void IsSendGridAccessible_WithOptionsTimeoutTimeNull_ReturnsTrueAfterTimeoutTimePassed()
    {
        // Arrange
        var emailOptions = Options.Create(new EmailOptions());
        var service = new SendGridAccessibilityService(emailOptions);
        var now = DateTimeOffset.Now;

        // Act
        service.SetSendGridInaccessible(now);

        var earlyResult = service.IsSendGridAccessible(now + TimeSpan.FromMinutes(250));
        var result = service.IsSendGridAccessible(now + TimeSpan.FromMinutes(301));

        // Assert
        Assert.True(earlyResult);
        Assert.True(result);
    }
}