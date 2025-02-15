using OutOfSchool.AikomApiClient.Models.Contract;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient.Models.Responses;

internal class SearchUniversityResponse : ApiResponse<SearchUniversityResponseData>, IResponse
{
}

internal class SearchUniversityResponseData
{
    public required int Id { get; set; }

    public required string UniversityFullName { get; set; }

    public bool? IsBranch { get; set; } = null; 

    public required string Edrpou { get; set; }

    public List<Branch>? Branches { get; set; }
}

internal class Branch
{
    public required int BranchId { get; set; }

    public required string BranchName { get; set; }

    public string? Edrpou { get; set; }
}