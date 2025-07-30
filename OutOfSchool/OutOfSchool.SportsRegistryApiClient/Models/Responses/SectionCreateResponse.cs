using OutOfSchool.Common.Models;

namespace OutOfSchool.SportsRegistryApiClient.Models.Responses;

public class SectionCreateResponse : IResponse
{
    public ResultVariables ResultVariables { get; set; } = null!;
}

public class ResultVariables
{
    public string Code { get; set; } = null!;
    public Guid SectionId { get; set; }
    public object? Errors { get; set; }
}