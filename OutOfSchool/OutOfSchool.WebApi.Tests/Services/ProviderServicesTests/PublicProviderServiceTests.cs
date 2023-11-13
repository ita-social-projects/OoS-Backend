﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
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
using OutOfSchool.WebApi.Services.AverageRatings;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Services.ProviderServices;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Services.ProviderServicesTests;

[TestFixture]
public class PublicProviderServiceTests
{
    private PublicProviderService publicProviderService;

    private Mock<IProviderRepository> providersRepositoryMock;
    private Mock<IProviderAdminRepository> providerAdminRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> usersRepositoryMock;
    private IMapper mapper;
    private Mock<INotificationService> notificationService;
    private Mock<IProviderAdminService> providerAdminService;
    private Mock<IInstitutionAdminRepository> institutionAdminRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<IRegionAdminRepository> regionAdminRepositoryMock;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IAreaAdminService> areaAdminServiceMock;

    private List<Provider> fakeProviders;
    private User fakeUser;

    [SetUp]
    public void SetUp()
    {
        fakeProviders = ProvidersGenerator.Generate(10);
        fakeUser = UserGenerator.Generate();

        providersRepositoryMock = ProviderTestsHelper.CreateProvidersRepositoryMock(fakeProviders);

        // TODO: configure mock and writer tests for provider admins
        providerAdminRepositoryMock = new Mock<IProviderAdminRepository>();
        usersRepositoryMock = ProviderTestsHelper.CreateUsersRepositoryMock(fakeUser);
        var addressRepo = new Mock<IEntityRepositorySoftDeleted<long, Address>>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        var logger = new Mock<ILogger<ProviderService>>();
        var workshopServicesCombiner = new Mock<IWorkshopServicesCombiner>();
        var providerImagesService = new Mock<IImageDependentEntityImagesInteractionService<Provider>>();
        var changesLogService = new Mock<IChangesLogService>();
        notificationService = new Mock<INotificationService>(MockBehavior.Strict);
        providerAdminService = new Mock<IProviderAdminService>();
        institutionAdminRepositoryMock = new Mock<IInstitutionAdminRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        regionAdminRepositoryMock = new Mock<IRegionAdminRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();


        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();

        publicProviderService = new PublicProviderService(
            providersRepositoryMock.Object,
            usersRepositoryMock.Object,
            logger.Object,
            localizer.Object,
            mapper,
            addressRepo.Object,
            workshopServicesCombiner.Object,
            providerAdminRepositoryMock.Object,
            providerImagesService.Object,
            changesLogService.Object,
            notificationService.Object,
            providerAdminService.Object,
            institutionAdminRepositoryMock.Object,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            regionAdminServiceMock.Object,
            codeficatorServiceMock.Object,
            regionAdminRepositoryMock.Object,
            averageRatingServiceMock.Object,
            areaAdminServiceMock.Object);
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
        notificationService
            .Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                dto.ProviderId,
                publicProviderService,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Callback((
                NotificationType type,
                NotificationAction action,
                Guid objectId,
                INotificationReciever service,
                Dictionary<string, string> additionalData,
                string groupedData) => service.GetNotificationsRecipientIds(action, additionalData, objectId))
            .Returns(Task.CompletedTask);

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

    #endregion
}
