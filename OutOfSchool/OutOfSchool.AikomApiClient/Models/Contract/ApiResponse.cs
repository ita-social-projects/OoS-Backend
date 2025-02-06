namespace OutOfSchool.AikomApiClient.Models.Contract;

public class ApiResponse<TData> where TData : class
{
    public required ResultVariables<TData> ResultVariables { get; set; }
}