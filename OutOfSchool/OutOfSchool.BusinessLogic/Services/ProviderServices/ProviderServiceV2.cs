using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.ProviderServices;

public class ProviderServiceV2(
    IProviderRepository providerRepository,
    IEntityRepositorySoftDeleted<string, User> usersRepository,
    ILogger<ProviderServiceV2> logger,
    IStringLocalizer<SharedResource> localizer,
    IEntityRepositorySoftDeleted<long, Address> addressRepository,
    ISensitiveEntityRepositorySoftDeleted<Individual> individualRepository,
    IOfficialRepository officialRepository,
    IPositionRepository positionRepository,
    IWorkshopServicesCombiner workshopServiceCombiner,
    IImageDependentEntityImagesInteractionService<Provider> providerImagesService,
    IChangesLogService changesLogService,
    INotificationService notificationService,
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
    ISearchStringService searchStringService,
    IContactsService<Provider, IHasContactsDto<Provider>> contactsService
) : ProviderService(
    providerRepository,
    usersRepository,
    logger,
    localizer,
    addressRepository,
    individualRepository,
    officialRepository,
    positionRepository,
    workshopServiceCombiner,
    providerImagesService,
    changesLogService,
    notificationService,
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
    searchStringService,
    contactsService
), IProviderServiceV2
{
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

    public new Task<Either<ErrorResponse, bool>> Delete(Guid id, string token)
    {
        return Task.FromResult<Either<ErrorResponse, bool>>(
            new ErrorResponse()
            {
                Message = "Deleting provider is not allowed.",
                HttpStatusCode = HttpStatusCode.Forbidden,
            });
        // TODO: do not allow provider deletion while requirements are updated
        // async Task BeforeDeleteAction(Provider provider)
        // {
        //     if (provider.Images?.Count > 0)
        //     {
        //         await ProviderImagesService.RemoveManyImagesAsync(provider, provider.Images.Select(x => x.ExternalStorageId).ToList()).ConfigureAwait(false);
        //     }
        //
        //     if (!string.IsNullOrEmpty(provider.CoverImageId))
        //     {
        //         await ProviderImagesService.RemoveCoverImageAsync(provider).ConfigureAwait(false);
        //     }
        // }
        //
        // return await DeleteProviderWithActionBefore(id, BeforeDeleteAction).ConfigureAwait(false);
    }
}