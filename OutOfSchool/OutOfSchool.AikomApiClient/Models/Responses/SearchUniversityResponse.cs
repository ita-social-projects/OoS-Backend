using OutOfSchool.AikomApiClient.Models.Contract;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient.Models.Responses;

public class SearchUniversityResponse : ApiResponse<SearchUniversityResponseData>, IResponse
{
}

public class SearchUniversityResponseData
{
    public required int Id { get; set; }

    public required string UniversityFullName { get; set; }

    public bool? IsBranch { get; set; } = null; 

    public required string Edrpou { get; set; }

    public List<Branch>? Branches { get; set; }
}

public class Branch
{
    public required int BranchId { get; set; }

    public required string BranchName { get; set; }

    public string? Edrpou { get; set; }
}