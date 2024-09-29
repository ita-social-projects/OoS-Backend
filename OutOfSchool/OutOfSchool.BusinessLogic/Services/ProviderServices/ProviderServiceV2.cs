using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.ProviderServices;

public class ProviderServiceV2 : ProviderService, IProviderServiceV2
{
    public ProviderServiceV2(
        IProviderRepository providerRepository,
        IEntityRepositorySoftDeleted<string, User> usersRepository,
        ILogger<ProviderServiceV2> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        IEntityRepositorySoftDeleted<long, Address> addressRepository,
        IWorkshopServicesCombiner workshopServiceCombiner,
        IEmployeeRepository employeeRepository,
        IImageDependentEntityImagesInteractionService<Provider> providerImagesService,
        IChangesLogService changesLogService,
        INotificationService notificationService,
        IEmployeeService employeeService,
        IInstitutionAdminRepository institutionAdminRepository,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        ICodeficatorService codeficatorService,
        IRegionAdminRepository regionAdminRepository,
        IAverageRatingService averageRatingService,
        IAreaAdminService areaAdminService,
        IAreaAdminRepository areaAdminRepository,
        IUserService userService,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        ICommunicationService communicationService,
        ISearchStringService searchStringService)
        : base(
              providerRepository,
              usersRepository,
              logger,
              localizer,
              mapper,
              addressRepository,
              workshopServiceCombiner,
              employeeRepository,
              providerImagesService,
              changesLogService,
              notificationService,
              employeeService,
              institutionAdminRepository,
              currentUserService,
              ministryAdminService,
              regionAdminService,
              codeficatorService,
              regionAdminRepository,
              averageRatingService,
              areaAdminService,
              areaAdminRepository,
              userService,
              authorizationServerConfig,
              communicationService,
              searchStringService)
    {
    }

    /// <inheritdoc cref="IProviderServiceV2" />
    public new async Task<ProviderDto> Create(ProviderCreateDto providerDto)
    {
        async Task AfterCreationAction(Provider provider)
        {
            if (providerDto.ImageFiles?.Count > 0)
            {
                provider.Images = new List<Image<Provider>>();
                await ProviderImagesService.AddManyImagesAsync(provider, providerDto.ImageFiles)
                    .ConfigureAwait(false);
            }

            if (providerDto.CoverImage != null)
            {
                await ProviderImagesService.AddCoverImageAsync(provider, providerDto.CoverImage)
                    .ConfigureAwait(false);
            }
        }

        return await CreateProviderWithActionAfterAsync(providerDto, AfterCreationAction).ConfigureAwait(false);
    }

    public new async Task<ProviderDto> Update(ProviderUpdateDto providerDto, string userId)
    {
        async Task BeforeUpdateAction(Provider provider)
        {
            await ProviderImagesService.ChangeImagesAsync(provider, providerDto.ImageIds ?? new List<string>(), providerDto.ImageFiles)
                .ConfigureAwait(false);

            await ProviderImagesService.ChangeCoverImageAsync(provider, providerDto.CoverImageId, providerDto.CoverImage).ConfigureAwait(false);
        }

        return await UpdateProviderWithActionBeforeSavingChanges(providerDto, userId, BeforeUpdateAction)
            .ConfigureAwait(false);
    }

    public new async Task<Either<ErrorResponse, ActionResult>> Delete(Guid id, string token)
    {
        async Task BeforeDeleteAction(Provider provider)
        {
            if (provider.Images?.Count > 0)
            {
                await ProviderImagesService.RemoveManyImagesAsync(provider, provider.Images.Select(x => x.ExternalStorageId).ToList()).ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(provider.CoverImageId))
            {
                await ProviderImagesService.RemoveCoverImageAsync(provider).ConfigureAwait(false);
            }
        }

        return await DeleteProviderWithActionBefore(id, token, BeforeDeleteAction).ConfigureAwait(false);
    }
}