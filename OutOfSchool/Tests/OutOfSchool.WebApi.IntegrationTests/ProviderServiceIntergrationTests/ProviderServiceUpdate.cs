namespace OutOfSchool.WebApi.IntegrationTests.ProviderServiceIntergrationTests;

using OutOfSchool.WebApi.Services.Images;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Services.AverageRatings;

[TestFixture]
public class ProviderServiceUpdate
{
    private IProviderService providerService;

    private Mapper mapper;
    private DbContextOptions<OutOfSchoolDbContext> unitTestDbOptions;

    private OutOfSchoolDbContext GetContext() => new OutOfSchoolDbContext(this.unitTestDbOptions);

    [SetUp]
    public async Task SetUp()
    {
        this.unitTestDbOptions = UnitTestHelper.GetUnitTestDbOptions();

        await using (var context = this.GetContext())
        {
            await context.SaveChangesAsync();
            var fakeProvider = ProvidersGenerator.Generate();
            context.Add(fakeProvider);
            await context.SaveChangesAsync();
        }

        this.mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(typeof(MappingProfile))));

        var localizer = new Mock<IStringLocalizer<SharedResource>>();
        var logger = new Mock<ILogger<ProviderService>>();
        var addressRepository = new Mock<IEntityRepository<long, Address>>();
        var providerRepository = new ProviderRepository(GetContext());
        var providerAdminRepository = new Mock<IProviderAdminRepository>();
        var userRepository = new Mock<IEntityRepository<string, User>>();
        var workshopServicesCombiner = new Mock<IWorkshopServicesCombiner>();
        var providerImagesService = new Mock<IImageDependentEntityImagesInteractionService<Provider>>();
        var changesLogService = new Mock<IChangesLogService>();
        var notificationService = new Mock<INotificationService>();
        var providerAdminService = new Mock<IProviderAdminService>();
        var institutionAdminRepository = new Mock<IInstitutionAdminRepository>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        var regionAdminService = new Mock<IRegionAdminService>();
        var codeficatorService = new Mock<ICodeficatorService>();
        var regionAdminRepository = new Mock<IRegionAdminRepository>();
        var averageRatingService = new Mock<IAverageRatingService>();
        var areaAdminServiceMock=new Mock<IAreaAdminService>();

        this.providerService = new ProviderService(
            providerRepository,
            userRepository.Object,
            logger.Object,
            localizer.Object,
            this.mapper,
            addressRepository.Object,
            workshopServicesCombiner.Object,
            providerAdminRepository.Object,
            providerImagesService.Object,
            changesLogService.Object,
            notificationService.Object,
            providerAdminService.Object,
            institutionAdminRepository.Object,
            currentUserServiceMock.Object,
            ministryAdminServiceMock.Object,
            regionAdminService.Object,
            codeficatorService.Object,
            regionAdminRepository.Object,
            averageRatingService.Object,
            areaAdminServiceMock.Object);
    }

    [Test]
    public async Task UpdateWhenProviderHasSameAdresses_WithSameAddresses_UpdatesOneAddress()
    {
        // Arrange
        await using var context = this.GetContext();

        var provider = context.Providers.First();
        provider.ActualAddressId = null;

        await context.SaveChangesAsync().ConfigureAwait(false);
        provider.ActualAddress = provider.LegalAddress;

        // Act
        var result = await this.providerService
            .Update(this.mapper.Map<ProviderUpdateDto>(provider), provider.UserId)
            .ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result.LegalAddress);
        Assert.IsNull(result.ActualAddress);
    }

    [Test]
    public async Task UpdateWhenProviderHasSameAdresses_WithActualAddressNull_UpdatesOneAddress()
    {
        // Arrange
        await using var context = this.GetContext();

        var provider = context.Providers.First();
        provider.ActualAddressId = null;

        await context.SaveChangesAsync().ConfigureAwait(false);

        var providerDto = this.mapper.Map<ProviderUpdateDto>(provider);
        providerDto.ActualAddress = null;

        // Act
        var result = await this.providerService.Update(providerDto, provider.UserId)
            .ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result.LegalAddress);
        Assert.IsNull(result.ActualAddress);
    }

    [Test]
    public async Task UpdateWhenProviderHasSameAdresses_WithNewLegalAddressAndActualIsPreviousLegal_UpdatesOneAddress()
    {
        // Arrange
        await using var context = this.GetContext();
        var provider = context.Providers.First();
        provider.ActualAddressId = null;
        await context.SaveChangesAsync().ConfigureAwait(false);

        var randomAddressToAdd = GenerateAddressToAdd();

        provider.LegalAddress = randomAddressToAdd;
        var providerDto = this.mapper.Map<ProviderUpdateDto>(provider);

        // Act
        var result = await this.providerService.Update(providerDto, provider.UserId)
            .ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result.LegalAddress);
        Assert.IsNull(result.ActualAddress);
    }

    [Test]
    public async Task UpdateWhenProviderHasSameAdresses_WithSameLegalAddressAndNewActual_UpdatesOneAddress()
    {
        // Arrange
        await using var context = this.GetContext();
        var provider = context.Providers.First();
        provider.ActualAddressId = null;
        await context.SaveChangesAsync().ConfigureAwait(false);

        var randomAddressToAdd = GenerateAddressToAdd();

        provider.ActualAddress = randomAddressToAdd;
        var providerDto = this.mapper.Map<ProviderUpdateDto>(provider);

        // Act
        var result = await this.providerService.Update(providerDto, provider.UserId)
            .ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result.LegalAddress);
        Assert.IsNotNull(result.ActualAddress);
    }

    [Test]
    public async Task UpdateWhenProviderDifferentActualAdresses_WithSameLegalAddressAndNewActual_RemovesActual()
    {
        // Arrange
        await using var context = this.GetContext();
        var provider = context.Providers.First();
        provider.ActualAddress = provider.LegalAddress;
        var providerDto = this.mapper.Map<ProviderUpdateDto>(provider);

        // Act
        var result = await this.providerService.Update(providerDto, provider.UserId)
            .ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result.LegalAddress);
        Assert.IsNull(result.ActualAddress);
    }

    private static Address GenerateAddressToAdd()
    {
        var randomAddress = AddressGenerator.Generate();
        randomAddress.Id = 0;
        return randomAddress;
    }
}