namespace OutOfSchool.AikomApiClient.Models.Contract;

internal class ApiResponse<TData> where TData : class
{
    public required ResultVariables<TData> ResultVariables { get; set; }
}