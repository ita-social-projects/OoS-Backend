namespace OutOfSchool.AikomApiClient.Models.Contract;

public class StartVariables<TData> where TData : class
{
    public required TData Request { get; set; }
}