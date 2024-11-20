using OutOfSchool.AikomApiClient.Models;

namespace OutOfSchool.AikomApiClient;

public interface IAikomApiService
{
    Task<ResponseDto> SearchUniversity(string edrpou);

    Task<ResponseDto> GetUniversity(int id);
}