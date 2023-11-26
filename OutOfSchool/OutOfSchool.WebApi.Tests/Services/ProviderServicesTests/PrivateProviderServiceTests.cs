using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.ProviderServices;

namespace OutOfSchool.WebApi.Tests.Services.ProviderServicesTests;

[TestFixture]
public class PrivateProviderServiceTests
{
    private PrivateProviderService privateProviderService;

    private Mock<IProviderRepository> providersRepositoryMock;
    private Mock<INotificationService> notificationService;
    private Mock<IProviderService> providerServiceMock;

    private List<Provider> fakeProviders;
    private User fakeUser;

    [SetUp]
    public void SetUp()
    {
        fakeProviders = ProvidersGenerator.Generate(10);
        fakeUser = UserGenerator.Generate();

        providersRepositoryMock = ProviderTestsHelper.CreateProvidersRepositoryMock(fakeProviders);

        // TODO: configure mock and writer tests for provider admins
        var logger = new Mock<ILogger<ProviderService>>();
        providerServiceMock = new Mock<IProviderService>();

        privateProviderService = new PrivateProviderService(
            providersRepositoryMock.Object,
            logger.Object,
            providerServiceMock.Object);
    }

    #region UpdateLicenseStatus

    [Test]
    public async Task UpdateLicenseStatus_WhenDtoIsValid_UpdatesProviderEntity()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();
        provider.License = TestDataHelper.GetPositiveInt(10, 10).ToString();
        provider.LicenseStatus = ProviderLicenseStatus.Pending;
        var dto = new ProviderLicenseStatusDto
        {
            ProviderId = provider.Id,
            LicenseStatus = ProviderLicenseStatus.Approved,
        };

        providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
            .ReturnsAsync(It.IsAny<int>());
        providerServiceMock.Setup(p => p.SendNotification(provider, NotificationAction.Update, false, true)).Returns(Task.CompletedTask);
        
        // Act
        var result = await privateProviderService.UpdateLicenseStatus(dto, fakeUser.Id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(provider.LicenseStatus, dto.LicenseStatus);
    }

    [Test]
    public async Task UpdateLicenseStatus_WhenProviderIdIsNonExistent_ReturnsNull()
    {
        // Arrange
        var dto = new ProviderLicenseStatusDto
        {
            ProviderId = Guid.NewGuid(),
            LicenseStatus = ProviderLicenseStatus.Approved,
        };

        providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
            .ReturnsAsync(null as Provider);

        // Act
        var result = await privateProviderService.UpdateLicenseStatus(dto, fakeUser.Id).ConfigureAwait(false);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void UpdateLicenseStatus_WhenProviderLicenseIsNull_ThrowsException()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();
        provider.License = null;
        provider.LicenseStatus = ProviderLicenseStatus.NotProvided;
        // provider.License = TestDataHelper.GetPositiveInt(10, 10).ToString();
        var dto = new ProviderLicenseStatusDto
        {
            ProviderId = provider.Id,
            LicenseStatus = ProviderLicenseStatus.Approved,
        };

        providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
            .ReturnsAsync(provider);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await privateProviderService.UpdateLicenseStatus(dto, fakeUser.Id));
    }

    [Test]
    public void UpdateLicenseStatus_SetNotProvidedLicenseStatus_WhenProviderHasLicense_ThrowsException()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();
        provider.License = TestDataHelper.GetPositiveInt(10, 10).ToString();
        provider.LicenseStatus = ProviderLicenseStatus.Pending;
        var dto = new ProviderLicenseStatusDto
        {
            ProviderId = provider.Id,
            LicenseStatus = ProviderLicenseStatus.NotProvided,
        };

        providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
            .ReturnsAsync(provider);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await privateProviderService.UpdateLicenseStatus(dto, fakeUser.Id));
    }

    #endregion
}
