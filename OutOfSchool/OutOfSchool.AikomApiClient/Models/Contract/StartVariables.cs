namespace OutOfSchool.AikomApiClient.Models.Contract;

internal class StartVariables<TData> where TData : class
{
    public required TData Request { get; set; }
}