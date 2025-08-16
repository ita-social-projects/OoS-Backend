using System.Net;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
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

    public async Task<Either<ErrorResponse, SectionCreateResponse>> RegisterSectionAsync(SportsSectionPostRequest request)
    {
        if (request is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "Request payload is null."
            };
        }

        logger.LogInformation(
            $"Registering section in Sports Registry. With organization code: ={request.OrganizationCode}");
        var result = await apiService.CreateSectionAsync(request).ConfigureAwait(false);

        return result.Match<Either<ErrorResponse, SectionCreateResponse>>(
            error =>
            {
                var message = string.IsNullOrWhiteSpace(error.Message)
                    ? "Sports Registry returned errors."
                    : error.Message;

                logger.LogWarning(
                    "Sports Registry error. Status={Status}, Message={Message}, OrgCode={OrgCode}",
                    error.HttpStatusCode, message, request.OrganizationCode);

                return new ErrorResponse
                {
                    HttpStatusCode = error.HttpStatusCode,
                    Message = message
                };
            },
            success =>
            {
                logger.LogInformation(
                    "Section registered successfully. OrgCode={OrgCode}, SectionId created in external API={ExternalId}",
                    request.OrganizationCode,
                    success.ResultVariables.SectionId);
                return success;
            });
    }
}