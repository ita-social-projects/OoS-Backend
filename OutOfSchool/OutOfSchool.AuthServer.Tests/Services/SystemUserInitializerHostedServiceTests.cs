using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.AuthServer.Tests.Services;
[TestFixture]
public class SystemUserInitializerHostedServiceTests
{
    private Mock<ILogger<SystemUserInitializerHostedService>> mockedLogger;
    private Mock<ISystemUserInitializer> mockedInitializer;
    private Mock<IServiceProvider> mockedServiceProvider;
    private SystemUserInitializerHostedService initializerHostedService;

    [SetUp]
    public void SetUp()
    {
        mockedLogger = new Mock<ILogger<SystemUserInitializerHostedService>>();
        mockedInitializer = new Mock<ISystemUserInitializer>();
        mockedServiceProvider = new Mock<IServiceProvider>();

        initializerHostedService = new SystemUserInitializerHostedService(
            mockedServiceProvider.Object,
            mockedLogger.Object);
    }

    [Test]
    public async Task StartAsync_WhenCalled_ShouldInvokeEnsureExistsAsync()
    {
        // Arrange
        var mockedServiceScope = new Mock<IServiceScope>();
        var mockedServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockedServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockedServiceScopeFactory.Object);
        mockedServiceScopeFactory.Setup(f => f.CreateScope())
            .Returns(mockedServiceScope.Object);
        mockedServiceScope.Setup(s => s.ServiceProvider.GetService(typeof(ISystemUserInitializer)))
            .Returns(mockedInitializer.Object);
        initializerHostedService = new SystemUserInitializerHostedService(
            mockedServiceProvider.Object,
            mockedLogger.Object);

        // Act
        await initializerHostedService.StartAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        mockedInitializer.Verify(i => i.EnsureExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task StopAsync_WhenCalled_ShouldCompleteImmediately()
    {
        // Act
        var task = initializerHostedService.StopAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(task.IsCompleted);
        await task;
    }
}
