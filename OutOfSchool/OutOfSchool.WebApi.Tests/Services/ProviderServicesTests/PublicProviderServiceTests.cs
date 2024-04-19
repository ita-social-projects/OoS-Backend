using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.ProviderServices;

namespace OutOfSchool.WebApi.Tests.Services.ProviderServicesTests;

[TestFixture]
public class PublicProviderServiceTests
{
    private PublicProviderService publicProviderService;

    private Mock<IProviderRepository> providersRepositoryMock;
    private Mock<IProviderService> providerServiceMock;
    private Mock<IChangesLogService> changesLogServiceMock;

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
        changesLogServiceMock = new Mock<IChangesLogService>();

        publicProviderService = new PublicProviderService(
            providersRepositoryMock.Object,
            logger.Object,
            providerServiceMock.Object,
            changesLogServiceMock.Object);
    }

    #region UpdateStatus

    [Test]
    public async Task UpdateStatus_WhenDtoIsValid_UpdatesProviderEntity()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();
        provider.Status = ProviderStatus.Pending;
        var dto = new ProviderStatusDto
        {
            ProviderId = provider.Id,
            Status = ProviderStatus.Approved,
        };

        providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
            .ReturnsAsync(It.IsAny<int>());

        // Act
        var result = await publicProviderService.UpdateStatus(dto, fakeUser.Id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(provider.Status, dto.Status);
    }

    [Test]
    public async Task UpdateStatus_WhenProviderIdIsNonExistent_ReturnsNull()
    {
        // Arrange
        var dto = new ProviderStatusDto
        {
            ProviderId = Guid.NewGuid(),
            Status = ProviderStatus.Approved,
        };

        providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
            .ReturnsAsync(null as Provider);

        // Act
        var result = await publicProviderService.UpdateStatus(dto, fakeUser.Id).ConfigureAwait(false);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task UpdateStatus_WhenDtoIsValid_UpdateAddedInChangesLog()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();
        provider.Status = ProviderStatus.Pending;
        var dto = new ProviderStatusDto
        {
            ProviderId = provider.Id,
            Status = ProviderStatus.Approved,
        };

        providersRepositoryMock.Setup(r => r.GetById(dto.ProviderId))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.UnitOfWork.CompleteAsync())
            .ReturnsAsync(It.IsAny<int>());

        changesLogServiceMock.Setup(
                s => s.AddEntityChangesToDbContext(
                    It.Is<Provider>(p => p.Id == dto.ProviderId),
                    It.Is(fakeUser.Id, StringComparer.Ordinal)))
            .Returns(1);

        // Act
        var result = await publicProviderService.UpdateStatus(dto, fakeUser.Id).ConfigureAwait(false);

        // Assert
        changesLogServiceMock.Verify(
                s => s.AddEntityChangesToDbContext(
                    It.Is<Provider>(p => p.Id == dto.ProviderId),
                    It.Is(fakeUser.Id, StringComparer.Ordinal)),
                Times.Once);
    }

    #endregion
}
