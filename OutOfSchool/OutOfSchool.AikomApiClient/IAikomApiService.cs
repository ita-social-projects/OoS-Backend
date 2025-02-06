using OutOfSchool.AikomApiClient.Models.Data;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient;

public interface IAikomApiService
{
    Task<Either<ErrorResponse, SearchUniversityDto>> SearchUniversity(string edrpou);

    Task<Either<ErrorResponse, GetUniversityDto>> GetUniversity(int id);
}