using System.Net;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

namespace OutOfSchool.SportsRegistryApiClient.Services;

public class SportsRegistryProviderService : ISportsRegistryProviderService
{
    private readonly ISportsRegistryApiService apiService;
    private readonly ILogger<SportsRegistryProviderService> logger;

    public SportsRegistryProviderService(
        ISportsRegistryApiService apiService,
        ILogger<SportsRegistryProviderService> logger)
    {
        this.apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Either<ErrorResponse, SectionCreateUpdateResponse>> RegisterSectionAsync(
        SportsSectionPostRequest request)
    {
        logger.LogInformation($"Registering section in Sports Registry. With organization code: {request.OrganizationCode}");
        
        var result = await apiService.CreateSectionAsync(request).ConfigureAwait(false);

        return HandleResult(result,RegistryConstants.CreateAction);
    }
    public async Task<Either<ErrorResponse, SectionCreateUpdateResponse>> UpdateSectionAsync(
        SportsSectionUpdateRequest request)
    {
        logger.LogInformation(
            "Updating section with {sectionId} in Sports Registry.",request.SectionId.ToString());
        
        var result = await apiService.UpdateSectionAsync(request).ConfigureAwait(false);
        return HandleResult(result,  RegistryConstants.UpdateAction,  request.SectionId );
    }

    private Either<ErrorResponse, SectionCreateUpdateResponse> HandleResult(
        Either<ErrorResponse, SectionCreateUpdateResponse> result,
        string actionDescription,
        Guid? sectionId = null)
    {
        return result.Match<Either<ErrorResponse, SectionCreateUpdateResponse>>(
            error =>
            {
                var message = $"{error.Message}. List of errors: {error.Content}";

                logger.LogWarning(
                    "Sports Registry {Action} error with status = {Status}. Message={Message}",
                    actionDescription, error.HttpStatusCode, message);

                return new ErrorResponse
                {
                    HttpStatusCode = error.HttpStatusCode,
                    Message = message
                };
            },
            success =>
            {
                logger.LogInformation(
                    "Sports Registry {Action} succeeded. SectionId={SectionId}",
                    actionDescription,success.ResultVariables.SectionId?.ToString());
                
                return success;
            });
    }
}