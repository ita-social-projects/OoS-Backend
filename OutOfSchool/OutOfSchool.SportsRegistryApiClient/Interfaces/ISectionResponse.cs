using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

namespace OutOfSchool.SportsRegistryApiClient.Interfaces;

public interface ISectionResponse : IResponse
{
    ResultVariables ResultVariables { get; set; }
}