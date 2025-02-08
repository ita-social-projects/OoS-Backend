using OutOfSchool.AikomApiClient.Models.Data;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient;

internal class FakeAikomProviderService : IAikomProviderService
{
    public Task<Either<ErrorResponse, AikomProviderResponse?>> VerifyProviderAndDirectorAccess(string providerEdrpou, string directorRnokpp, string? cacheKeyPrefix = null,
        TimeSpan? cacheExpiration = null)
    {
        return Task.FromResult<Either<ErrorResponse, AikomProviderResponse?>>(
            new AikomProviderResponse
            {
                HasAccess = true,
                ProviderInfo = new AikomProviderInfo
                {
                    FullName = "Заклад освіти кошенят",
                    ShortName = "Мяу",
                    Address = "",
                    Email = "test@a.com",
                    Phone = "+380671234567"
                }
            });
    }

    public Task<Either<ErrorResponse, AikomProviderResponse?>> VerifyDirectorAccess(long externalRegistryProviderId, string directorRnokpp, string? cacheKeyPrefix = null,
        TimeSpan? cacheExpiration = null)
    {
        return Task.FromResult<Either<ErrorResponse, AikomProviderResponse?>>(
            new AikomProviderResponse
            {
                HasAccess = true,
                ProviderInfo = new AikomProviderInfo
                {
                    FullName = "Заклад освіти кошенят",
                    ShortName = "Мяу",
                    Address = "",
                    Email = "test@a.com",
                    Phone = "+380671234567"
                }
            });
    }
}