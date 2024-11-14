using System.Text.Json.Serialization;

namespace OutOfSchool.AikomApiClient.Models;

public class Branch
{
    public required int BranchId { get; set; }

    public required string BranchName { get; set; }

    public string? Edrpou { get; set; }
}

public class ResponseData
{
    public required int Id { get; set; }

    public required string UniversityFullName { get; set; }

    public bool? IsBranch { get; set; } = null; 

    public required string Edrpou { get; set; }

    public List<Branch>? Branches { get; set; }
}

public class Error
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required int Code { get; set; }

    public required string Message { get; set; }
}

public class Response
{
    public ResponseData? Data { get; set; }

    public Error? Error { get; set; }
}

public class ResultVariables
{
    public required Response Response { get; set; }
}

public class SearchUniversityResponse
{
    public required ResultVariables ResultVariables { get; set; }
}
