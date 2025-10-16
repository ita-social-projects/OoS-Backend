using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using System.Net;
using OutOfSchool.SportsRegistryApiClient.Models.External;

namespace OutOfSchool.SportsRegistryApiClient.Services;
public class SportsRegistryWorkshopProviderService : ISportsRegistryWorkshopProvider
{
    private readonly ISportsRegistryApiService apiService;
    private readonly ILogger<SportsRegistryWorkshopProviderService> logger;

    public SportsRegistryWorkshopProviderService(
        ISportsRegistryApiService apiService,
        ILogger<SportsRegistryWorkshopProviderService> logger)
    {
        this.apiService = apiService;
        this.logger = logger;
    }
   public async Task<Either<ErrorResponse, List<ExternalSportsSectionDto>>> GetAllSportsSectionsAsync(int pageSize = 50)
    {
        var all = new List<ExternalSportsSectionDto>();
        int currentPage = 0;
        int totalPages = 1;

        while (currentPage < totalPages)
        {
            var pageResult = await apiService.GetSectionsAsync(currentPage, pageSize);

            var failed = pageResult.Match(
                error =>
                {
                    logger.LogError("Failed to fetch sections page {Page}: {Message}",
                        currentPage, error.Message);
                    return true;
                },
                success =>
                {
                    var items = success.Content ?? new List<ExternalSportsSectionDto>();
                    all.AddRange(items);
                    totalPages = success.TotalPages;
                    return false;
                });

            if (failed)
            {
                return new ErrorResponse
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = $"Failed to fetch sections on page {currentPage}"
                };
            }

            currentPage++;
        }

        return all;
    }
}
