using System.Net;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;

namespace OutOfSchool.SportsRegistryApiClient.Services;

public class SportsRegistryDictionaryProvider : ISportsRegistryDictionaryProvider
{
    private readonly ISportsRegistryApiService apiService;
    private readonly ILogger<SportsRegistryDictionaryProvider> logger;

    public SportsRegistryDictionaryProvider(
        ISportsRegistryApiService apiService,
        ILogger<SportsRegistryDictionaryProvider> logger)
    {
        this.apiService = apiService;
        this.logger = logger;
    }

    public async Task<Either<ErrorResponse, List<SportKindDto>>> GetAllSportKindsAsync(int pageSize = 50)
    {
        var all = new List<SportKindDto>();
        int currentPage = 0;
        int totalPages = 1;

        while (currentPage < totalPages)
        {
            var pageResult = await apiService.GetSportKindsAsync(currentPage, pageSize);

            var failed = pageResult.Match(
                error =>
                {
                    logger.LogError("Failed to fetch page {Page}: {Message}", currentPage, error.Message);
                    return true;
                },
                success =>
                {
                    var items = success.Content ?? new List<SportKindDto>();
                    all.AddRange(items);
                    totalPages = success.TotalPages;
                    return false;
                });

            if (failed)
            {
                return new ErrorResponse
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = $"Failed to fetch sport kinds on page {currentPage}"
                };
            }

            currentPage++;
        }

        return all;
    }
}