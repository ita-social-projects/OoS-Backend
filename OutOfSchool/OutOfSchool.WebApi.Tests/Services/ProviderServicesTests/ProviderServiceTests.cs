using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services.ProviderServicesTests;

[TestFixture]
public class ProviderServiceTests
{
    private ProviderService providerService;

    private Mock<IProviderRepository> providersRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<string, User>> usersRepositoryMock;
    private Mock<INotificationService> notificationService;
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
    private Mock<IWorkshopServicesCombiner> workshopServicesCombinerMock;
    private Mock<ISearchStringService> searchStringServiceMock;
    private Mock<IOfficialRepository> officialRepo;

    private List<Provider> fakeProviders;
    private User fakeUser;

    [SetUp]
    public void SetUp()
    {
        fakeProviders = ProvidersGenerator.Generate(10);
        fakeUser = UserGenerator.Generate();

        providersRepositoryMock = new Mock<IProviderRepository>();

        usersRepositoryMock = ProviderTestsHelper.CreateUsersRepositoryMock(fakeUser);
        var addressRepo = new Mock<IEntityRepositorySoftDeleted<long, Address>>();
        var individualRepo = new Mock<ISensitiveEntityRepositorySoftDeleted<Individual>>();
        officialRepo = new Mock<IOfficialRepository>();
        var positionRepo = new Mock<IPositionRepository>();
        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        var logger = new Mock<ILogger<ProviderService>>();
        var providerImagesService = new Mock<IImageDependentEntityImagesInteractionService<Provider>>();
        var changesLogService = new Mock<IChangesLogService>();
        notificationService = new Mock<INotificationService>();
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
        workshopServicesCombinerMock = new Mock<IWorkshopServicesCombiner>();

        var authorizationServerConfig = Options.Create(new AuthorizationServerConfig { Authority = new Uri("http://test.com") });
        searchStringServiceMock = new Mock<ISearchStringService>();
        var providerContactsServiceMock = new Mock<IContactsService<Provider, IHasContactsDto<Provider>>>();

        providerService = new ProviderService(
            providersRepositoryMock.Object,
            usersRepositoryMock.Object,
            logger.Object,
            localizer.Object,
            addressRepo.Object,
            individualRepo.Object,
            officialRepo.Object,
            positionRepo.Object,
            workshopServicesCombinerMock.Object,
            providerImagesService.Object,
            changesLogService.Object,
            notificationService.Object,
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
            searchStringServiceMock.Object,
            providerContactsServiceMock.Object
            );
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

        var provider = dto.ToModel();
        provider.Status = ProviderStatus.Pending;
        provider.License = license;
        provider.LicenseStatus = expectedLicenseStatus;
        provider.CoverImageId = string.Empty;
        provider.Images = [];
        provider.ProviderSectionItems = [];

        var expected = provider.ToDto();

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.Create(It.IsAny<Provider>()))
            .ReturnsAsync((Provider p) => p);
        providersRepositoryMock.Setup(r => r.GetById(expected.Id))
            .ReturnsAsync(provider);
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
    public void Create_WhenInputProviderIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await providerService.Create(default).ConfigureAwait(false));
    }

    [Test]
    public void Create_WhenProviderWithTheSameEdrpouOrIpnExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = ProviderCreateDtoGenerator.Generate();
        providersRepositoryMock.Setup(r => r.SameExists(It.IsAny<Provider>())).Returns(true);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await providerService.Create(createDto));
    }

    #endregion

    #region GetByFilter

    [Ignore("Rating for providers is calculated according to rating of its workshops.")]
    [Test]
    public async Task GetByFilter_WhenCalled_ReturnsEntities()
    {
        // TODO: Need to add a generator for workshop rating
        // Arrange
        var filter = new ProviderFilter();
        var providersMock = fakeProviders.AsQueryable().BuildMock();

        var fakeRatings = RatingsGenerator.GetAverageRatings(fakeProviders.Select(p => p.Id)); // expected ratings
        var expected = fakeProviders.ToDto();
        expected
            .ForEach(p => p.Rating = fakeRatings
                .Where(r => r.EntityId == p.Id)
                .Select(p => p.Rate)
                .FirstOrDefault());
        expected
            .ForEach(p => p.NumberOfRatings = fakeRatings
                .Where(r => r.EntityId == p.Id)
                .Select(p => p.RateQuantity)
                .FirstOrDefault());
        providersRepositoryMock
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<Provider, bool>>>()))
            .ReturnsAsync(fakeRatings.Count);
        providersRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, dynamic>>, SortDirection>>()))
            .Returns(providersMock);
        averageRatingServiceMock.Setup(r => r.GetByEntityIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(fakeRatings);

        // Act
        var result = await providerService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
    }

    [Test]
    public async Task GetByFilter_WhenMinistryAdminCalled_ReturnsEntities()
    {
        // Arrange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        var filter = new ProviderFilter();
        var providersMock = fakeProviders.WithInstitutionId(institutionId).AsQueryable().BuildMock();

        currentUserServiceMock.Setup(c => c.IsMinistryAdmin()).Returns(true);
        ministryAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult(new MinistryAdminDto()
            {
                InstitutionId = institutionId,
            }));

        var fakeRatings = RatingsGenerator.GetAverageRatings(fakeProviders.Select(p => p.Id)); // expected ratings
        var expected = fakeProviders.ToDto();
        expected
            .ForEach(p => p.Rating = fakeRatings
                .Where(r => r.EntityId == p.Id)
                .Select(p => p.Rate)
                .FirstOrDefault());
        expected
            .ForEach(p => p.NumberOfRatings = fakeRatings
                .Where(r => r.EntityId == p.Id)
                .Select(p => p.RateQuantity)
                .FirstOrDefault());
        providersRepositoryMock
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<Provider, bool>>>()))
            .ReturnsAsync(fakeRatings.Count); // fakeRatings.Count);providersMock.Count
        providersRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, dynamic>>, SortDirection>>()))
            .Returns(providersMock);
        averageRatingServiceMock.Setup(r => r.GetByEntityIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(fakeRatings);

        // Act
        var result = await providerService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        Assert.True(result.Entities.All(p => p.InstitutionId == institutionId));
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
    }

    [Test]
    public async Task GetByFilter_WhenRegionAdminCalled_ReturnsEntities()
    {
        // Arrange
        var institutionId = new Guid("b929a4cd-ee3d-4bad-b2f0-d40aedf656c4");
        long catottgId = 31737;
        var filter = new ProviderFilter();
        var providersMock = fakeProviders.WithInstitutionId(institutionId).AsQueryable().BuildMock();

        currentUserServiceMock.Setup(c => c.IsRegionAdmin()).Returns(true);
        regionAdminServiceMock
            .Setup(m => m.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult(new RegionAdminDto()
            {
                InstitutionId = institutionId,
                CATOTTGId = catottgId,
            }));

        codeficatorServiceMock
            .Setup(x => x.GetAllChildrenIdsByParentIdAsync(It.IsAny<long>()))
            .Returns(Task.FromResult((IEnumerable<long>)new List<long> { catottgId }));

        var fakeRatings = RatingsGenerator.GetAverageRatings(fakeProviders.Select(p => p.Id)); // expected ratings
        var expected = fakeProviders.ToDto();
        expected
            .ForEach(p => p.Rating = fakeRatings
                .Where(r => r.EntityId == p.Id)
                .Select(p => p.Rate)
                .FirstOrDefault());
        expected
            .ForEach(p => p.NumberOfRatings = fakeRatings
                .Where(r => r.EntityId == p.Id)
                .Select(p => p.RateQuantity)
                .FirstOrDefault());
        providersRepositoryMock
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<Provider, bool>>>()))
            .ReturnsAsync(fakeRatings.Count); // fakeRatings.Count);providersMock.Count
        providersRepositoryMock
            .Setup(repo => repo.Get(
                filter.From,
                filter.Size,
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, dynamic>>, SortDirection>>()))
            .Returns(providersMock);
        averageRatingServiceMock.Setup(r => r.GetByEntityIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(fakeRatings);

        // Act
        var result = await providerService.GetByFilter(filter).ConfigureAwait(false);

        // Assert
        Assert.True(result.Entities.All(p => p.InstitutionId == institutionId));
        TestHelper.AssertTwoCollectionsEqualByValues(expected, result.Entities);
        Assert.AreEqual(fakeProviders.Count, result.TotalAmount);
    }

    [Test]
    public async Task GetByFilter_WhenFilteredBySearchString_ShouldReturnEntities()
    {
        // Arrange
        var filter = new ProviderFilter()
        {
            SearchString = "Провайдер Освітніх Програм",
        };

        fakeProviders[0].FullTitle = "Провайдер програм";
        fakeProviders[1].ShortTitle = "Провайдер";

        var filteredProviders = new List<Provider> { fakeProviders[0], fakeProviders[1] };
        var expectedDtos = filteredProviders.ToDto();
        var providerRatings = RatingsGenerator.GetAverageRatings(expectedDtos.Select(p => p.Id));

        SetupMocks(
            filter,
            filteredProviders,
            expectedDtos,
            providerRatings,
            ["Провайдер", "Освітніх", "Програм"]);

        // Act
        var result = await providerService.GetByFilter(filter)
            .ConfigureAwait(false);

        // Assert
        result.Entities.Should()
            .BeEquivalentTo(expectedDtos);

        searchStringServiceMock.VerifyAll();
        averageRatingServiceMock.VerifyAll();
    }

    #endregion

    #region GetById

    [Test]
    public async Task GetById_WhenIdIsValid_ReturnsEntity()
    {
        // Arrange
        var providersMock = fakeProviders.AsQueryable().BuildMock();
        var existingProvider = fakeProviders.First();

        providersRepositoryMock.Setup(r => r.Get(
                0,
                0,
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, dynamic>>, SortDirection>>()))
        .Returns(providersMock);

        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(existingProvider);
        averageRatingServiceMock.Setup(r => r.GetByEntityIdAsync(It.IsAny<Guid>())).ReturnsAsync(new AverageRatingDto());
        providersRepositoryMock.Setup(r => r.Any(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync(true);

        // Act
        var actualProviderDto = await providerService.GetById(existingProvider.Id).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(existingProvider.ToDto(), actualProviderDto);
        averageRatingServiceMock.Verify(r => r.GetByEntityIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task GetById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Arrange
        var noneExistingId = Guid.NewGuid();

        // Act
        var result = await providerService.GetById(noneExistingId).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region GetProviderStatusById

    [Test]
    public async Task GetProviderStatusById_WhenIdIsValid_ReturnsStatusDto()
    {
        // Arrange
        var existingProvider = fakeProviders.RandomItem();
        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(existingProvider);

        // Act
        var actualProviderStatusDto = await providerService.GetProviderStatusById(existingProvider.Id).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(existingProvider.ToStatusDto(), actualProviderStatusDto);
    }

    [Test]
    public async Task GetProviderStatusById_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Arrange
        var noneExistingId = Guid.NewGuid();

        // Act
        var result = await providerService.GetProviderStatusById(noneExistingId).ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region Update

    [Test]
    public async Task Update_UserCanUpdateExistingEntityOfRelatedProvider_UpdatesExistedEntity()
    {
        // Arrange
        var updatedTitle = Guid.NewGuid().ToString();
        var providerToUpdateDto = ProviderUpdateDtoGenerator.Generate();
        var provider = providerToUpdateDto.SetToModel(ProvidersGenerator.Generate());
        provider.Status = ProviderStatus.Recheck;
        var expectedProviderDto = provider.ToDto();
        providerToUpdateDto.FullTitle = updatedTitle;
        expectedProviderDto.FullTitle = updatedTitle;

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetWithNavigations(It.IsAny<Guid>()))
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
        var result = await providerService.Update(providerToUpdateDto, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expectedProviderDto, result);
    }

    [TestCase(ProviderStatus.Recheck)]
    [TestCase(ProviderStatus.Editing)]
    [TestCase(ProviderStatus.Approved)]
    public async Task Update_UserTriesToChangeStatus_StatusIsNotChanged(ProviderStatus initialStatus)
    {
        // Arrange
        var updatedTitle = Guid.NewGuid().ToString();
        var providerToUpdateDto = ProviderUpdateDtoGenerator.Generate();
        providerToUpdateDto.Status = ProviderStatus.Approved;
        providerToUpdateDto.ShortTitleEn = updatedTitle;

        var provider = providerToUpdateDto.SetToModel(ProvidersGenerator.Generate());
        provider.Status = initialStatus;

        var expected = provider.ToDto();
        expected.Status = initialStatus;
        expected.ShortTitleEn = updatedTitle;

        providersRepositoryMock.Setup(r => r.GetWithNavigations(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await providerService.Update(providerToUpdateDto, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [TestCase(ProviderStatus.Recheck)]
    [TestCase(ProviderStatus.Editing)]
    [TestCase(ProviderStatus.Approved)]
    public async Task Update_UserChangesFullTitle_StatusIsChangedToRecheck(ProviderStatus initialStatus)
    {
        // Arrange
        var providerToUpdateDto = ProviderUpdateDtoGenerator.Generate();
        providerToUpdateDto.Status = initialStatus;
        var provider = providerToUpdateDto.SetToModel(ProvidersGenerator.Generate());
        provider.Status = initialStatus;

        var updatedTitle = Guid.NewGuid().ToString();
        providerToUpdateDto.FullTitle = updatedTitle;

        var expected = provider.ToDto();
        expected.Status = ProviderStatus.Recheck;
        expected.FullTitle = updatedTitle;

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetWithNavigations(It.IsAny<Guid>()))
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
        var result = await providerService.Update(providerToUpdateDto, Guid.NewGuid().ToString()).ConfigureAwait(false);

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
        var providerToUpdateDto = ProviderUpdateDtoGenerator.Generate();
        var updatedLicense = "1234567890";
        providerToUpdateDto.License = updatedLicense;

        var provider = providerToUpdateDto.SetToModel(ProvidersGenerator.Generate());
        provider.License = license;
        provider.LicenseStatus = initialStatus;

        var expected = provider.ToDto();
        expected.LicenseStatus = ProviderLicenseStatus.Pending;
        expected.License = updatedLicense;

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetWithNavigations(It.IsAny<Guid>()))
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
        var result = await providerService.Update(providerToUpdateDto, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [TestCase(ProviderLicenseStatus.NotProvided)]
    [TestCase(ProviderLicenseStatus.Pending)]
    [TestCase(ProviderLicenseStatus.Approved)]
    public async Task Update_UserSetsLicenseToNull_LicenseStatusIsChangedToNotProvided(ProviderLicenseStatus initialStatus)
    {
        // Arrange
        var providerToUpdateDto = ProviderUpdateDtoGenerator.Generate();
        string updatedLicense = null;
        providerToUpdateDto.License = updatedLicense;

        var provider = providerToUpdateDto.SetToModel(ProvidersGenerator.Generate());
        provider.License = "1234512345";
        provider.LicenseStatus = initialStatus;

        var expected = provider.ToDto();
        expected.LicenseStatus = ProviderLicenseStatus.NotProvided;
        expected.License = updatedLicense;

        providersRepositoryMock.Setup(r => r.GetWithNavigations(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await providerService.Update(providerToUpdateDto, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expected, result);
    }

    [TestCase(OwnershipType.State)]
    [TestCase(OwnershipType.Common)]
    [TestCase(OwnershipType.Private)]
    public async Task Update_Always_KeepsOwnershipTypeNotChanged(OwnershipType ownershipType)
    {
        // Arrange
        var providerToUpdateDto = ProviderUpdateDtoGenerator.Generate();

        var provider = providerToUpdateDto.SetToModel(ProvidersGenerator.Generate());
        provider.Ownership = ownershipType;

        var expectedProviderDto = provider.ToDto();

        providersRepositoryMock.Setup(r => r.GetWithNavigations(It.IsAny<Guid>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await providerService.Update(providerToUpdateDto, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        TestHelper.AssertDtosAreEqual(expectedProviderDto, result);
    }

    #endregion

    #region Delete

    [Test]
    [Ignore("Delete is forbidden until requirements change.")]
    public async Task Delete_WhenIdIsValid_CalledProvidersRepositoryDeleteMethod()
    {
        // Arrange
        var providerToDeleteDto = fakeProviders.RandomItem().ToDto();
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
        await providerService.Delete(providerToDeleteDto.Id).ConfigureAwait(false);
        var result = deleteMethodArguments.Single().ToDto();

        // Assert
        TestHelper.AssertDtosAreEqual(providerToDeleteDto, result);
    }

    [Test]
    public async Task Delete_WhenIdIsInvalid_ReturnNotFoundErrorResponse()
    {
        // Arrange
        var fakeProviderInvalidId = Guid.NewGuid();

        // Act
        var result = await providerService.Delete(fakeProviderInvalidId).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, result.Match(left => HttpStatusCode.NotFound, right => HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_WhenUserHasNoRights_ReturnForbiddenErrorResponse()
    {
        // Arrange
        var providerToDeleteId = Guid.NewGuid();
        providersRepositoryMock.Setup(p => p.GetWithNavigations(providerToDeleteId)).ReturnsAsync(new Provider() { Id = providerToDeleteId});
        currentUserServiceMock.Setup(p => p.UserId).Returns(string.Empty);
        currentUserServiceMock.Setup(p => p.IsAdmin()).Returns(false);

        // Act
        var result = await providerService.Delete(providerToDeleteId).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, result.Match(left => HttpStatusCode.Forbidden, right => HttpStatusCode.OK));
    }

    #endregion

    #region GetProviderIdForWorkshopById

    [Test]
    public async Task GetProviderIdForWorkshopById_MethodWasCalled()
    {
        // Arrange
        var existingProvider = fakeProviders.RandomItem();
        existingProvider.WithWorkshops();
        workshopServicesCombinerMock.Setup(w => w.GetWorkshopProviderId(existingProvider.Workshops.FirstOrDefault().Id)).ReturnsAsync(existingProvider.Id);

        // Act
        var actualResult = await providerService.GetProviderIdForWorkshopById(existingProvider.Workshops.FirstOrDefault().Id);

        // Assert
        workshopServicesCombinerMock.Verify(w => w.GetWorkshopProviderId(existingProvider.Workshops.FirstOrDefault().Id), Times.Once);
        Assert.AreEqual(existingProvider.Id, actualResult);
    }

    #endregion

    #region Block

    [Test]
    public async Task Block_ReturnsNull_IfDtoIsNotValid()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();

        var providerBlockDto = new ProviderBlockDto()
        {
            Id = provider.Id,
            IsBlocked = true,
            BlockReason = "Test reason",
        };

        providersRepositoryMock.Setup(r => r.GetById(providerBlockDto.Id))
            .ReturnsAsync(null as Provider);

        // Act
        var result = await providerService.Block(providerBlockDto).ConfigureAwait(false);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.NotFound, result.HttpStatusCode);
        Assert.IsNull(result.Result);
    }

    [Test]
    public async Task Block_ReturnsProviderBlockDto_IfDtoIsValid()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();

        var providerBlockDto = new ProviderBlockDto()
        {
            Id = provider.Id,
            IsBlocked = true,
            BlockReason = "Test reason",
        };

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Provider>,IQueryable<Provider>>>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>());
        providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<ProviderBlockDto>>>()))
            .ReturnsAsync(providerBlockDto);
        currentUserServiceMock.Setup(x => x.IsAdmin())
            .Returns(true);
        currentUserServiceMock.Setup(x => x.IsMinistryAdmin())
            .Returns(false);
        currentUserServiceMock.Setup(x => x.IsRegionAdmin())
            .Returns(false);
        currentUserServiceMock.Setup(x => x.IsAreaAdmin())
            .Returns(false);
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Block,
                provider.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);
        officialRepo.Setup(s => s.GetDirectorOfficialUserIdByProviderIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(fakeUser.Id);
        // Act
        var result = await providerService.Block(providerBlockDto).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
        Assert.AreEqual(providerBlockDto, result.Result);
    }

    [Test]
    public async Task Block_ReturnsProviderBlockDto_IfMinistryAdminHasRights()
    {
        // Arrange
        var institutionId = Guid.NewGuid();

        var provider = ProvidersGenerator.Generate();
        provider.InstitutionId = institutionId;

        var ministryAdmin = AdminGenerator.GenerateMinistryAdminDto();
        ministryAdmin.InstitutionId = institutionId;

        var providerBlockDto = new ProviderBlockDto()
        {
            Id = provider.Id,
            IsBlocked = true,
            BlockReason = "Test reason",
        };

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Provider>,IQueryable<Provider>>>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>());
        providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<ProviderBlockDto>>>()))
            .ReturnsAsync(providerBlockDto);
        ministryAdminServiceMock.Setup(x => x.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult(ministryAdmin));
        currentUserServiceMock.Setup(x => x.IsAdmin())
            .Returns(true);
        currentUserServiceMock.Setup(x => x.IsMinistryAdmin())
            .Returns(true);
        currentUserServiceMock.Setup(x => x.IsAreaAdmin())
            .Returns(false);
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Block,
                provider.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);
        officialRepo.Setup(s => s.GetDirectorOfficialUserIdByProviderIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(fakeUser.Id);
        // Act
        var result = await providerService.Block(providerBlockDto).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
        Assert.AreEqual(providerBlockDto, result.Result);
    }

    [Test]
    public async Task Block_ReturnsProviderBlockDto_IfRegionAdminHasRights()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var catottgId = 12345;

        var provider = ProvidersGenerator.Generate();
        provider.InstitutionId = institutionId;
        var contacts = new Contacts
        {
            Title = "Test",
            IsDefault = true,
            Address = ContactsAddressGenerator.Generate(),
        };
        contacts.Address.CATOTTGId = catottgId;
        provider.Contacts = [contacts];

        var regionAdmin = AdminGenerator.GenerateRegionAdminDto();
        regionAdmin.InstitutionId = institutionId;
        regionAdmin.CATOTTGId = catottgId;

        var providerBlockDto = new ProviderBlockDto()
        {
            Id = provider.Id,
            IsBlocked = true,
            BlockReason = "Test reason",
        };

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Provider>,IQueryable<Provider>>>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>());
        providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<ProviderBlockDto>>>()))
            .ReturnsAsync(providerBlockDto);
        regionAdminServiceMock.Setup(x => x.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult(regionAdmin));
        currentUserServiceMock.Setup(x => x.IsAdmin())
            .Returns(true);
        currentUserServiceMock.Setup(x => x.IsRegionAdmin())
            .Returns(true);
        currentUserServiceMock.Setup(x => x.IsAreaAdmin())
            .Returns(false);
        codeficatorServiceMock.Setup(x => x.GetAllChildrenIdsByParentIdAsync(It.IsAny<long>()))
            .Returns(Task.FromResult((IEnumerable<long>)new List<long> { catottgId }));
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Block,
                provider.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);
        officialRepo.Setup(s => s.GetDirectorOfficialUserIdByProviderIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(fakeUser.Id);
        // Act
        var result = await providerService.Block(providerBlockDto).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
        Assert.AreEqual(providerBlockDto, result.Result);
    }

    [Test]
    public async Task Block_ReturnsProviderBlockDto_IfAreaAdminHasRights()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var catottgId = 12345;

        var provider = ProvidersGenerator.Generate();
        provider.InstitutionId = institutionId;
        var contacts = new Contacts
        {
            Title = "Test",
            IsDefault = true,
            Address = ContactsAddressGenerator.Generate(),
        };
        contacts.Address.CATOTTGId = catottgId;
        provider.Contacts = [contacts];

        var areaAdmin = AdminGenerator.GenerateAreaAdminDto();
        areaAdmin.InstitutionId = institutionId;
        areaAdmin.CATOTTGId = catottgId;

        var providerBlockDto = new ProviderBlockDto()
        {
            Id = provider.Id,
            IsBlocked = true,
            BlockReason = "Test reason",
        };

        var recipientsIds = new List<string>() { fakeUser.Id };

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Provider>,IQueryable<Provider>>>()))
            .ReturnsAsync(provider);
        providersRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>());
        providersRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<ProviderBlockDto>>>()))
            .ReturnsAsync(providerBlockDto);
        areaAdminServiceMock.Setup(x => x.GetByUserId(It.IsAny<string>()))
            .Returns(Task.FromResult(areaAdmin));
        currentUserServiceMock.Setup(x => x.IsAdmin())
            .Returns(true);
        currentUserServiceMock.Setup(x => x.IsRegionAdmin())
            .Returns(false);
        currentUserServiceMock.Setup(x => x.IsAreaAdmin())
            .Returns(true);
        codeficatorServiceMock.Setup(x => x.GetAllChildrenIdsByParentIdAsync(It.IsAny<long>()))
            .Returns(Task.FromResult((IEnumerable<long>)new List<long> { catottgId }));
        notificationService.Setup(s => s.Create(
                NotificationType.Provider,
                NotificationAction.Block,
                provider.Id,
                recipientsIds,
                It.IsAny<Dictionary<string, string>>(),
                null))
            .Returns(Task.CompletedTask);
        officialRepo.Setup(s => s.GetDirectorOfficialUserIdByProviderIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(fakeUser.Id);
        // Act
        var result = await providerService.Block(providerBlockDto).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
        Assert.AreEqual(providerBlockDto, result.Result);
    }

    [Test]
    public async Task Block_ReturnsNull_IfNotAdmin()
    {
        // Arrange
        var provider = ProvidersGenerator.Generate();

        var providerBlockDto = new ProviderBlockDto()
        {
            Id = provider.Id,
            IsBlocked = true,
            BlockReason = "Test reason",
        };

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Provider>,IQueryable<Provider>>>()))
            .ReturnsAsync(provider);

        currentUserServiceMock.Setup(x => x.IsAdmin())
            .Returns(false);

        // Act
        var result = await providerService.Block(providerBlockDto).ConfigureAwait(false);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.Forbidden, result.HttpStatusCode);
        Assert.IsNull(result.Result);
    }

    #endregion Block

    #region IsBlocked

    [Test]
    public async Task IsBlocked_WhenIdIsValid_ReturnsResult()
    {
        // Arrange
        var existingProvider = fakeProviders.RandomItem();
        providersRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(existingProvider);

        // Act
        var actualResult = await providerService.IsBlocked(existingProvider.Id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(existingProvider.IsBlocked, actualResult);
    }

    [Test]
    public async Task IsBlocked_WhenNoRecordsInDbWithSuchId_ReturnsNullAsync()
    {
        // Arrange
        var noneExistingId = new Guid();

        // Act
        var actualResult = await providerService.IsBlocked(noneExistingId).ConfigureAwait(false);

        // Assert
        Assert.IsNull(actualResult);
    }

    #endregion

    #region SendNotification

    [Test]
    public async Task SendNotification_WithValidProvider_ShouldCallNotificationServiceCreate()
    {
        // Arrange
        var existingProvider = fakeProviders.RandomItem();
        var notificationAction = NotificationAction.Create;

        var mockAddress = new ContactsAddress { CATOTTGId = 1 };
        var mockContact = new Contacts { IsDefault = true, Address = mockAddress };
        existingProvider.Contacts = new List<Contacts> { mockContact };

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(
                existingProvider.Id,
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Provider>, IQueryable<Provider>>>()))
            .ReturnsAsync(existingProvider);

        // Act
        await providerService.SendNotification(existingProvider, notificationAction, false, false);

        // Assert
        notificationService.Verify(n => n.Create(
            NotificationType.Provider,
            notificationAction,
            existingProvider.Id,
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<Dictionary<string, string>>(),
            null), Times.Once);
    }

    [Test]
    public async Task SendNotification_WithNoRecipients_ShouldNotCallNotificationServiceCreate()
    {
        // Arrange
        var existingProvider = fakeProviders.RandomItem();
        var notificationAction = NotificationAction.Unknown;

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(
                existingProvider.Id,
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Provider>, IQueryable<Provider>>>()))
            .ReturnsAsync(existingProvider);

        // Act
        await providerService.SendNotification(existingProvider, notificationAction, false, false);

        // Assert
        notificationService.Verify(n => n.Create(
            It.IsAny<NotificationType>(),
            It.IsAny<NotificationAction>(),
            It.IsAny<Guid>(),
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendNotification_WithStatusData_ShouldAddStatusToAdditionalData()
    {
        // Arrange
        var existingProvider = fakeProviders.RandomItem();
        existingProvider.Status = ProviderStatus.Approved;
        var notificationAction = NotificationAction.Create;

        var mockAddress = new ContactsAddress { CATOTTGId = 1 };
        var mockContact = new Contacts { IsDefault = true, Address = mockAddress };
        existingProvider.Contacts = new List<Contacts> { mockContact };

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(
                existingProvider.Id,
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Provider>, IQueryable<Provider>>>()))
            .ReturnsAsync(existingProvider);

        var mockTechAdmins = new List<User>
    {
        new User { Id = "tech-admin-1", Role = "techadmin" },
        new User { Id = "tech-admin-2", Role = "techadmin" }
    };

        usersRepositoryMock.Setup(r => r.GetByFilter(
                It.Is<Expression<Func<User, bool>>>(expr => expr.ToString().Contains("techadmin")),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>()))
            .ReturnsAsync(mockTechAdmins);

        // Act
        await providerService.SendNotification(existingProvider, notificationAction, true, false);

        // Assert
        notificationService.Verify(n => n.Create(
            NotificationType.Provider,
            notificationAction,
            existingProvider.Id,
            It.IsAny<IEnumerable<string>>(),
            It.Is<Dictionary<string, string>>(d => d.ContainsKey("Status") && d["Status"] == "Approved"),
            null), Times.Once);
    }

    [Test]
    public async Task SendNotification_WithLicenseStatusData_ShouldAddLicenseStatusToAdditionalData()
    {
        // Arrange
        var existingProvider = fakeProviders.RandomItem();
        existingProvider.LicenseStatus = ProviderLicenseStatus.Approved;
        var notificationAction = NotificationAction.Create;

        var mockAddress = new ContactsAddress { CATOTTGId = 1 };
        var mockContact = new Contacts { IsDefault = true, Address = mockAddress };
        existingProvider.Contacts = new List<Contacts> { mockContact };

        providersRepositoryMock.Setup(r => r.GetByIdWithDetails(
                existingProvider.Id,
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Provider>, IQueryable<Provider>>>()))
            .ReturnsAsync(existingProvider);

        var mockTechAdmins = new List<User>
    {
        new User { Id = "tech-admin-1", Role = "techadmin" }
    };

        usersRepositoryMock.Setup(r => r.GetByFilter(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>()))
            .ReturnsAsync(mockTechAdmins);

        // Act
        await providerService.SendNotification(existingProvider, notificationAction, false, true);

        // Assert
        notificationService.Verify(n => n.Create(
            NotificationType.Provider,
            notificationAction,
            existingProvider.Id,
            It.IsAny<IEnumerable<string>>(),
            It.Is<Dictionary<string, string>>(d => d.ContainsKey("LicenseStatus") && d["LicenseStatus"] == "Approved"),
            null), Times.Once);
    }

    [Test]
    public async Task SendNotification_ShouldNotCallMethod_WhenProviderDoesntExists()
    {
        // Arrange
        var notificationAction = new NotificationAction();
        notificationService.Setup(n => n.Create(It.IsAny<NotificationType>(), notificationAction,
            It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Dictionary<string, string>>(), null));

        // Act
        providerService.SendNotification(null, notificationAction,
            It.IsAny<bool>(), It.IsAny<bool>());

        // Assert
        notificationService.Verify(n => n.Create(It.IsAny<NotificationType>(), notificationAction,
            It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Dictionary<string, string>>(), null), Times.Never);
    }

    #endregion

    #region UpdateWorkshopsProviderStatus

    [Test]
    public async Task UpdateWorkshopsProviderStatus_MethodWasCalled()
    {
        // Arrange
        var existingProvider = fakeProviders.RandomItem();
        var providerStatus = new ProviderStatus();
        workshopServicesCombinerMock.Setup(n => n.UpdateProviderStatus(existingProvider.Id, providerStatus)).ReturnsAsync(new List<ShortEntityDto>());

        // Act
        await providerService.UpdateWorkshopsProviderStatus(existingProvider.Id, providerStatus);

        // Assert
        workshopServicesCombinerMock.Verify(n => n.UpdateProviderStatus(existingProvider.Id, providerStatus), Times.Once);
    }

    #endregion

    #region TestDataSets

    private static IEnumerable<object> AdditionalTestData()
    {
        yield return new Dictionary<string, string> { { nameof(Provider.Status), ProviderStatus.Approved.ToString() }, };
        yield return new Dictionary<string, string> { { nameof(Provider.Status), ProviderStatus.Editing.ToString() }, };
        yield return new Dictionary<string, string> { { nameof(Provider.LicenseStatus), ProviderLicenseStatus.Approved.ToString() }, };
        yield return new Dictionary<string, string>
        {
            { nameof(Provider.Status), ProviderStatus.Pending.ToString() },
            { nameof(Provider.LicenseStatus), ProviderLicenseStatus.Approved.ToString() },
        };
        yield return new Dictionary<string, string>
        {
            { nameof(Provider.Status), ProviderStatus.Approved.ToString() },
            { nameof(Provider.LicenseStatus), ProviderLicenseStatus.NotProvided.ToString() },
        };
        yield return new Dictionary<string, string>
        {
            { nameof(Provider.Status), ProviderStatus.Approved.ToString() },
            { nameof(Provider.LicenseStatus), ProviderLicenseStatus.Approved.ToString() },
        };
    }

    #endregion

    #region SetupMocks
    private void SetupMocks(
        ProviderFilter filter = null,
        List<Provider> filteredProviders = null,
        List<ProviderDto> expectedDtos = null,
        IEnumerable<AverageRatingDto> providerRatings = null,
        string[] searchWords = null)
    {
        searchStringServiceMock
            .Setup(s => s.SplitSearchString(It.Is<string>(x => x == filter.SearchString)))
            .Returns(searchWords);

        providersRepositoryMock
            .Setup(repo => repo.Count(It.IsAny<Expression<Func<Provider, bool>>>()))
            .ReturnsAsync(filteredProviders.Count);

        providersRepositoryMock
            .Setup(repo => repo.Get(
                It.Is<int>(x => x == filter.From),
                It.Is<int>(x => x == filter.Size),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Provider, dynamic>>, SortDirection>>()))
            .Returns(filteredProviders.AsQueryable()
            .BuildMock());

        averageRatingServiceMock
           .Setup(r => r.GetByEntityIdsAsync(It.Is<IEnumerable<Guid>>(x =>
                x.SequenceEqual(expectedDtos.Select(p => p.Id)))))
          .ReturnsAsync(providerRatings);
    }
    #endregion
}