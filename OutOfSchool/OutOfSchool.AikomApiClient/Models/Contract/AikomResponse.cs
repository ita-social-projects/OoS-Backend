namespace OutOfSchool.AikomApiClient.Models.Contract;

public class AikomResponse<TData> where TData : class
{
    public TData? Data { get; set; }
    public AikomError? Error { get; set; }
}