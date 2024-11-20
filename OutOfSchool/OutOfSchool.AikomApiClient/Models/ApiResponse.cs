using System.Text.Json.Serialization;

namespace OutOfSchool.AikomApiClient.Models;

public class ApiResponse<TData> where TData : class
{
    public required ResultVariables<TData> ResultVariables { get; set; }
}

public class ResultVariables<TData> where TData : class
{
    public required Response<TData> Response { get; set; }
}

public class Response<TData> where TData : class
{
    public TData? Data { get; set; }
    public Error? Error { get; set; }
}

public class Error
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public required int Code { get; set; }

    public required string Message { get; set; }
}
