using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;

namespace OutOfSchool.SportsRegistryApiClient.Interfaces;

public interface ISportsRegistryDictionaryProvider
{
    Task<Either<ErrorResponse, List<SportKindDto>>> GetAllSportKindsAsync(int pageSize = 50);
}