namespace OutOfSchool.AikomApiClient.Models.Contract;

public class ResultVariables<TData> where TData : class
{
    public required AikomResponse<TData> Response { get; set; }
}