using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Interfaces;

namespace OutOfSchool.SportsRegistryApiClient.Models.Responses;

public class SectionCreateUpdateResponse : ISectionResponse
{
    public ResultVariables ResultVariables { get; set; } = null!;
}