namespace OutOfSchool.AikomApiClient.Models.Contract;

internal class ResultVariables<TData> where TData : class
{
    public required AikomResponse<TData> Response { get; set; }
}