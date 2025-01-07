using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.ProviderServicesTests;

[TestFixture]
public class ProviderServiceV2Tests
{
    private ProviderServiceV2 providerService;

    private Mock<IProviderRepository> providersRepositoryMock;
    private Mock<IEmployeeRepository> providerAdminRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> usersRepositoryMock;
    private IMapper mapper;
    private Mock<INotificationService> notificationService;
    private Mock<IEmployeeService> providerAdminService;
    private Mock<IInstitutionAdminRepository> institutionAdminRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private Mock<ICodeficatorService> codeficatorServiceMock;
    private Mock<IRegionAdminRepository> regionAdminRepositoryMock;
    private Mock<IAverageRatingService> averageRatingServiceMock;
    private Mock<IAreaAdminService> areaAdminServiceMock;
    private Mock<IAreaAdminRepository> areaAdminRepositoryMock;
    private Mock<IUserService> userServiceMock;
    private Mock<ICommunicationService> communicationService;
    private Mock<IImageDependentEntityImagesInteractionService<Provider>> providerImagesService;

    private List<Provider> fakeProviders;
    private User fakeUser;

    [SetUp]
    public void SetUp()
    {
        fakeProviders = ProvidersGenerator.Generate(10);
        fakeUser = UserGenerator.Generate();

        providersRepositoryMock = ProviderTestsHelper.CreateProvidersRepositoryMock(fakeProviders);

        // TODO: configure mock and writer tests for provider admins
        providerAdminRepositoryMock = new Mock<IEmployeeRepository>();
        usersRepositoryMock = ProviderTestsHelper.CreateUsersRepositoryMock(fakeUser);
        var addressRepo = new Mock<IEntityRepositorySoftDeleted<long, Address>>();
        var individualRepo = new Mock<IEntityRepositorySoftDeleted<Guid, Individual>>();
        var officialRepo = new Mock<IEntityRepositorySoftDeleted<Guid, Official>>();
        var positionRepo = new Mock<IEntityRepositorySoftDeleted<Guid, Position>>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        var logger = new Mock<ILogger<ProviderServiceV2>>();
        var workshopServicesCombiner = new Mock<IWorkshopServicesCombiner>();
        var changesLogService = new Mock<IChangesLogService>();
        notificationService = new Mock<INotificationService>(MockBehavior.Strict);
        providerAdminService = new Mock<IEmployeeService>();
        institutionAdminRepositoryMock = new Mock<IInstitutionAdminRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        codeficatorServiceMock = new Mock<ICodeficatorService>();
        regionAdminRepositoryMock = new Mock<IRegionAdminRepository>();
        averageRatingServiceMock = new Mock<IAverageRatingService>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();
        areaAdminRepositoryMock = new Mock<IAreaAdminRepository>();
        userServiceMock = new Mock<IUserService>();
        communicationService = new Mock<ICommunicationService>();
        providerImagesService = new Mock<IImageDependentEntityImagesInteractionService<Provider>>();

        var authorizationServerConfig = Options.Create(new AuthorizationServerConfig { Authority = new Uri("http://test.com") });
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        var searchStringServiceMock = new Mock<ISearchStringService>();

        providerService = new ProviderServiceV2(
            providersRepositoryMock.Object,
            usersRepositoryMock.Object,
            logger.Object,
            localizer.Object,
            mapper,
            addressRepo.Object,
            individualRepo.Object,
            officialRepo.Object,
            positionRepo.Object,
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
            areaAdminServiceMock.Object,
            areaAdminRepositoryMock.Object,
            userServiceMock.Object,
            authorizationServerConfig,
            communicationService.Object,
            searchStringServiceMock.Object);
        providersRepositoryMock.Setup(w => w.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(It.IsAny<int>());
    }

    #region Create

    [TestCase(null, ProviderLicenseStatus.NotProvided)]
    [TestCase("1234567890", ProviderLicenseStatus.Pending)]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity(string license, ProviderLicenseStatus expectedLicenseStatus)
    {
        // Arrange
        var dto = ProviderCreateDtoGenerator.Generate();
        dto.License = license;
        dto.Status = ProviderStatus.Approved;

        var expected = mapper.Map<ProviderDto>(dto);
        expected.Status = ProviderStatus.Pending;
        expected.License = license;
        expected.LicenseStatus = expectedLicenseStatus;
        expected.CoverImageId = null;
        expected.ImageIds = new List<string>();
        expected.ProviderSectionItems = Enumerable.Empty<ProviderSectionItemDto>();

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.Create(It.IsAny<Provider>()))
            .ReturnsAsync((Provider p) => p);
        providersRepositoryMock.Setup(r => r.GetById(expected.Id))
            .ReturnsAsync(mapper.Map<Provider>(expected));
        notificationService
            .Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Create,
                expected.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await providerService.Create(dto).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task Create_ValidEntity_UpdatesOwnerIsRegisteredField()
    {
        // Arrange
        var provider = ProviderCreateDtoGenerator.Generate();
        provider.UserId = fakeUser.Id;
        fakeUser.IsRegistered = false;

        // Act
        await providerService.Create(provider).ConfigureAwait(false);

        // Assert
        Assert.That(fakeUser.IsRegistered, Is.True);
    }

    [Test]
    public void Create_WhenInputProviderIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await providerService.Create(default).ConfigureAwait(false));
    }

    [Test]
    public void Create_WhenUserIdExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var providerToBeCreated = ProviderCreateDtoGenerator.Generate();
        fakeProviders.RandomItem().UserId = providerToBeCreated.UserId;

        // Act and Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await providerService.Create(providerToBeCreated).ConfigureAwait(false));
    }

    [Test]
    public async Task Create_WhenActualAddressIsTheSameAsLegal_ActualAddressIsCleared()
    {
        // Arrange
        var expectedEntity = ProviderCreateDtoGenerator.Generate();
        expectedEntity.ActualAddress = expectedEntity.LegalAddress;
        Provider receivedProvider = default;
        providersRepositoryMock.Setup(x => x.Create(It.IsAny<Provider>())).
            Callback<Provider>(p => receivedProvider = p);

        // Act
        await providerService.Create(expectedEntity).ConfigureAwait(false);

        // Assert
        Assert.That(receivedProvider.ActualAddress, Is.Null);
    }

    [Test]
    public async Task Create_WhenActualAddressDiffersFromTheLegal_ActualAddressIsSaved()
    {
        // Arrange
        var expectedEntity = ProvidersGenerator.Generate();
        expectedEntity.ActualAddress.CATOTTGId = 4970;
        expectedEntity.ActualAddress.CATOTTG.Id = 4970;

        Provider receivedProvider = default;
        providersRepositoryMock.Setup(x => x.Create(It.IsAny<Provider>())).Callback<Provider>(p => receivedProvider = p);

        // Act
        await providerService.Create(mapper.Map<ProviderCreateDto>(expectedEntity));// expectedEntity.ToModel());

        // Assert
        Assert.That(receivedProvider.ActualAddress, Is.Not.Null);
        Assert.AreEqual(expectedEntity.ActualAddress, receivedProvider.ActualAddress);
    }

    [Test]
    public void Create_WhenProviderWithTheSameEdrpouOrIpnExist_ThrowsInvalidOperationException()
    {
        // Arrange
        providersRepositoryMock.Setup(r => r.SameExists(It.IsAny<Provider>())).Returns(true);
        var randomProvider = mapper.Map<ProviderCreateDto>(fakeProviders.RandomItem());// fakeProviders.RandomItem().ToModel();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await providerService.Create(randomProvider));
    }

    [Test]
    public async Task Create_ShouldCallAddMayImagesMethod_WhenHasImagesFiles()
    {
        // Arrange
        var expectedEntity = ProvidersGenerator.Generate();
        var expectedEntityDto = mapper.Map<ProviderCreateDto>(expectedEntity);
        var file = new Mock<IFormFile>().Object;
        expectedEntityDto.ImageFiles = new List<IFormFile> { file };

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(p => p.Create(It.IsAny<Provider>())).ReturnsAsync(expectedEntity);
        providersRepositoryMock.Setup(r => r.GetById(expectedEntity.Id)).ReturnsAsync(mapper.Map<Provider>(expectedEntity));
        providerImagesService.Setup(p => p.AddManyImagesAsync(expectedEntity, expectedEntityDto.ImageFiles)).ReturnsAsync(new MultipleImageUploadingResult());
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Create,
                It.IsAny<Guid>(),
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null)).Returns(Task.CompletedTask);

        // Act
        await providerService.Create(expectedEntityDto).ConfigureAwait(false);

        // Assert
        providerImagesService.Verify(p => p.AddManyImagesAsync(expectedEntity, expectedEntityDto.ImageFiles), Times.Once);
    }

    [Test]
    public async Task Create_ShouldCallCoverImageMethod_WhenHasCoverImage()
    {
        // Arrange
        var expectedEntity = ProvidersGenerator.Generate();
        var expectedEntityDto = mapper.Map<ProviderCreateDto>(expectedEntity);
        var file = new Mock<IFormFile>().Object;
        expectedEntityDto.CoverImage = file;

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(p => p.Create(It.IsAny<Provider>())).ReturnsAsync(expectedEntity);
        providersRepositoryMock.Setup(r => r.GetById(expectedEntity.Id)).ReturnsAsync(mapper.Map<Provider>(expectedEntity));
        providerImagesService.Setup(p => p.AddCoverImageAsync(expectedEntity, expectedEntityDto.CoverImage)).ReturnsAsync(new Result<string>());
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Create,
                It.IsAny<Guid>(),
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null)).Returns(Task.CompletedTask);

        // Act
        await providerService.Create(expectedEntityDto).ConfigureAwait(false);

        // Assert
        providerImagesService.Verify(p => p.AddCoverImageAsync(expectedEntity, expectedEntityDto.CoverImage), Times.Once);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_UserCanUpdateExistingEntityOfRelatedProvider_UpdatesExistedEntity()
    {
        // Arrange
        var provider = fakeProviders.RandomItem();
        provider.Status = ProviderStatus.Recheck;

        var updatedTitle = Guid.NewGuid().ToString();
        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(provider);
        var expectedProviderDto = mapper.Map<ProviderDto>(provider);
        providerToUpdateDto.FullTitle = updatedTitle;
        expectedProviderDto.FullTitle = updatedTitle;

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Provider>>>()))
            .Returns((Func<Task<Provider>> f) => f.Invoke());
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                provider.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expectedProviderDto, result);
    }

    [TestCase(ProviderStatus.Recheck)]
    [TestCase(ProviderStatus.Editing)]
    [TestCase(ProviderStatus.Approved)]
    public async Task Update_UserTriesToChangeStatus_StatusIsNotChanged(ProviderStatus initialStatus)
    {
        // Arrange
        var provider = fakeProviders.RandomItem();
        var updatedTitle = Guid.NewGuid().ToString();
        provider.Status = initialStatus;

        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(provider);
        providerToUpdateDto.Status = ProviderStatus.Approved;
        providerToUpdateDto.ShortTitle = updatedTitle;

        var expected = mapper.Map<ProviderDto>(provider);
        expected.Status = initialStatus;
        expected.ShortTitle = updatedTitle;

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [TestCase(ProviderStatus.Recheck)]
    [TestCase(ProviderStatus.Editing)]
    [TestCase(ProviderStatus.Approved)]
    public async Task Update_UserChangesFullTitle_StatusIsChangedToRecheck(ProviderStatus initialStatus)
    {
        // Arrange
        var provider = fakeProviders.RandomItem();
        var updatedTitle = Guid.NewGuid().ToString();
        provider.Status = initialStatus;

        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(provider);
        providerToUpdateDto.FullTitle = updatedTitle;

        var expected = mapper.Map<ProviderDto>(provider);
        expected.Status = ProviderStatus.Recheck;
        expected.FullTitle = updatedTitle;

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Provider>>>()))
            .Returns((Func<Task<Provider>> f) => f.Invoke());
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                provider.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [TestCase(ProviderStatus.Recheck)]
    [TestCase(ProviderStatus.Editing)]
    [TestCase(ProviderStatus.Approved)]
    public async Task Update_UserChangesEdrpouIpn_StatusIsChangedToRecheck(ProviderStatus initialStatus)
    {
        // Arrange
        var provider = fakeProviders.RandomItem();
        provider.EdrpouIpn = "1234512345";
        var updatedEdrpouIpn = "1234567890";
        provider.Status = initialStatus;

        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(provider);
        providerToUpdateDto.EdrpouIpn = updatedEdrpouIpn;

        var expected = mapper.Map<ProviderDto>(provider);
        expected.Status = ProviderStatus.Recheck;
        expected.EdrpouIpn = updatedEdrpouIpn;

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                provider.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [TestCase(ProviderLicenseStatus.NotProvided, "1234512345")]
    [TestCase(ProviderLicenseStatus.Pending, "1234512345")]
    [TestCase(ProviderLicenseStatus.Approved, "1234512345")]
    [TestCase(ProviderLicenseStatus.NotProvided, null)]
    [TestCase(ProviderLicenseStatus.Pending, null)]
    [TestCase(ProviderLicenseStatus.Approved, null)]
    public async Task Update_UserChangesLicense_LicenseStatusIsChangedToPending(ProviderLicenseStatus initialStatus, string license)
    {
        // Arrange
        var provider = fakeProviders.RandomItem();
        provider.License = license;
        var updatedLicense = "1234567890";
        provider.LicenseStatus = initialStatus;

        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(provider);
        providerToUpdateDto.License = updatedLicense;

        var expected = mapper.Map<ProviderDto>(provider);
        expected.LicenseStatus = ProviderLicenseStatus.Pending;
        expected.License = updatedLicense;

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Update,
                provider.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [TestCase(ProviderLicenseStatus.NotProvided)]
    [TestCase(ProviderLicenseStatus.Pending)]
    [TestCase(ProviderLicenseStatus.Approved)]
    public async Task Update_UserSetsLicenseToNull_LicenseStatusIsChangedToNotProvided(ProviderLicenseStatus initialStatus)
    {
        // Arrange
        var provider = fakeProviders.RandomItem();
        provider.License = "1234512345";
        string updatedLicense = null;
        provider.LicenseStatus = initialStatus;

        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(provider);
        providerToUpdateDto.License = updatedLicense;

        var expected = mapper.Map<ProviderDto>(provider);
        expected.LicenseStatus = ProviderLicenseStatus.NotProvided;
        expected.License = updatedLicense;

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [Test]
    public async Task Update_WhenUsersIdAndProvidersIdDoesntMatch_ReturnsNull()
    {
        // Arrange
        var changedEntity = mapper.Map<ProviderUpdateDto>(ProviderDtoGenerator.Generate());
        var noneExistingUserId = Guid.NewGuid().ToString();

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(fakeProviders.RandomItem());

        // Act
        var result = await providerService.Update(changedEntity, noneExistingUserId).ConfigureAwait(false);

        // Assert
        Assert.Null(result);
    }

    [TestCase(OwnershipType.State)]
    [TestCase(OwnershipType.Common)]
    [TestCase(OwnershipType.Private)]
    public async Task Update_Always_KeepsOwnershipTypeNotChanged(OwnershipType ownershipType)
    {
        // Arrange
        var provider = fakeProviders.RandomItem();
        provider.Ownership = ownershipType;

        var providerToUpdateDto = mapper.Map<ProviderUpdateDto>(provider);
        var expectedProviderDto = mapper.Map<ProviderDto>(provider);

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await providerService.Update(providerToUpdateDto, providerToUpdateDto.UserId).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expectedProviderDto, result);
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_WhenIdIsValid_CalledProvidersRepositoryDeleteMethod()
    {
        // Arrange
        var providerToDeleteDto = mapper.Map<ProviderDto>(fakeProviders.RandomItem());
        var deleteMethodArguments = new List<Provider>();
        var deleteUserArguments = new List<string>();
        providersRepositoryMock.Setup(r => r.GetWithNavigations(It.IsAny<Guid>()))
            .ReturnsAsync(fakeProviders.Single(p => p.Id == providerToDeleteDto.Id));
        providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> f) => f.Invoke());
        providersRepositoryMock.Setup(r => r.Delete(Capture.In(deleteMethodArguments)));
        userServiceMock.Setup(r => r.Delete(Capture.In(deleteUserArguments)));
        currentUserServiceMock.Setup(p => p.IsAdmin()).Returns(true);
        communicationService.Setup(x => x.SendRequest<ResponseDto, ErrorResponse>(It.IsAny<Request>(), null)).ReturnsAsync(new ResponseDto());

        // Act
        await providerService.Delete(providerToDeleteDto.Id, It.IsAny<string>()).ConfigureAwait(false);
        var userToDeleteId = deleteUserArguments.Single();
        var result = mapper.Map<ProviderDto>(deleteMethodArguments.Single());

        // Assert
        TestHelper.AssertDtosAreEqual(providerToDeleteDto, result);
        Assert.AreEqual(providerToDeleteDto.UserId, userToDeleteId);
    }

    [Test]
    public async Task Delete_WhenIdIsInvalid_ReturnNotFoundErrorResponse()
    {
        // Arrange
        var fakeProviderInvalidId = Guid.NewGuid();

        // Act
        var result = await providerService.Delete(fakeProviderInvalidId, It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, result.Match(left => HttpStatusCode.NotFound, right => HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_WhenUserHasNoRights_ReturnForbiddenErrorResponse()
    {
        // Arrange
        Guid providerToDeleteId = Guid.NewGuid();
        string providerToDeleteUserId = fakeUser.Id;
        providersRepositoryMock.Setup(p => p.GetWithNavigations(providerToDeleteId)).ReturnsAsync(new Provider() { UserId = providerToDeleteUserId });
        currentUserServiceMock.Setup(p => p.UserId).Returns(string.Empty);
        currentUserServiceMock.Setup(p => p.IsAdmin()).Returns(false);

        // Act
        var result = await providerService.Delete(providerToDeleteId, It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, result.Match(left => HttpStatusCode.Forbidden, right => HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_WhenUserHasRightsAndIdIsValid_CommunicationServiceSendsRequest()
    {
        // Arrange
        currentUserServiceMock.Setup(p => p.IsAdmin()).Returns(true);
        providersRepositoryMock.Setup(r => r.GetWithNavigations(It.IsAny<Guid>()))
            .ReturnsAsync(fakeProviders.FirstOrDefault());
        communicationService.Setup(x => x.SendRequest<ResponseDto, ErrorResponse>(It.IsAny<Request>(), null)).ReturnsAsync(new ResponseDto());

        // Act
        await providerService.Delete(It.IsAny<Guid>(), It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        communicationService.Verify(x => x.SendRequest<ResponseDto, ErrorResponse>(It.IsAny<Request>(), null), Times.AtLeastOnce);
    }

    [Test]
    public async Task Delete_ShouldCallRemoveManyImagesMethod_WhenHasImagesFiles()
    {
        // Arrange
        var images = ImagesGenerator.Generate<Provider>(5);
        var imageIds = images.Select(i => i.ExternalStorageId).ToList();
        var providerToDelete = ProvidersGenerator.Generate();
        var providerToDeleteDto = mapper.Map<ProviderDto>(providerToDelete);
        providerToDelete.Images = images;

        var deleteMethodArguments = new List<Provider>();
        var deleteUserArguments = new List<string>();
        providersRepositoryMock.Setup(r => r.GetWithNavigations(providerToDelete.Id))
            .ReturnsAsync(providerToDelete);
        providersRepositoryMock.Setup(r => r.Delete(Capture.In(deleteMethodArguments)));
        userServiceMock.Setup(r => r.Delete(Capture.In(deleteUserArguments)));
        currentUserServiceMock.Setup(p => p.IsAdmin()).Returns(true);
        communicationService.Setup(x => x.SendRequest<ResponseDto, ErrorResponse>(It.IsAny<Request>(), null)).ReturnsAsync(new ResponseDto());

        // Act
        await providerService.Delete(providerToDelete.Id, It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        providerImagesService.Verify(p => p.RemoveManyImagesAsync(providerToDelete, imageIds), Times.Once);
    }

    [Test]
    public async Task Delete_ShouldCallRemoveCoverImageMethod_WhenHasCoverImage()
    {
        // Arrange
        var image = ImagesGenerator.Generate<Provider>();
        var providerToDelete = ProvidersGenerator.Generate();
        var providerToDeleteDto = mapper.Map<ProviderDto>(providerToDelete);
        providerToDelete.CoverImageId = image.EntityId.ToString();

        var deleteMethodArguments = new List<Provider>();
        var deleteUserArguments = new List<string>();
        providersRepositoryMock.Setup(r => r.GetWithNavigations(providerToDelete.Id))
            .ReturnsAsync(providerToDelete);
        providersRepositoryMock.Setup(r => r.Delete(Capture.In(deleteMethodArguments)));
        userServiceMock.Setup(r => r.Delete(Capture.In(deleteUserArguments)));
        currentUserServiceMock.Setup(p => p.IsAdmin()).Returns(true);
        communicationService.Setup(x => x.SendRequest<ResponseDto, ErrorResponse>(It.IsAny<Request>(), null)).ReturnsAsync(new ResponseDto());

        // Act
        await providerService.Delete(providerToDelete.Id, It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        providerImagesService.Verify(p => p.RemoveCoverImageAsync(providerToDelete), Times.Once);
    }

    #endregion
}
