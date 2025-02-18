using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient.Models.Contract;

internal class ApiResponse : IResponse
{
    public required ResultVariables ResultVariables { get; set; }
}