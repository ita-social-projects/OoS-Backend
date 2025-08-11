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
        logger.LogInformation("Registering new section in Sports Registry for organization: {OrganizationCode}", request.OrganizationCode);

        var result = await apiService.CreateSectionAsync(request).ConfigureAwait(false);

        return result.Match<Either<ErrorResponse, SectionCreateResponse>>(
            error => error,
            success =>
            {
                var code = success.ResultVariables?.Code;
                var errors = success.ResultVariables?.Errors;

                if (!IsSuccessStatusCode(code))
                {
                    return new ErrorResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = $"Registry returned error code {code}",
                        Content = errors
                    };
                }
                return success;
            });
    }

    private bool IsSuccessStatusCode(string? code)
    {
        return int.TryParse(code, out var statusCode) && statusCode is >= 200 and < 300;
    }
}