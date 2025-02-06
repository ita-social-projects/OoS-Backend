using System.Text.Json.Serialization;

namespace OutOfSchool.AikomApiClient.Models.Contract;

public abstract class ApiRequest<TData>(string businessProcessDefinitionKey, StartVariables<TData> startVariables)
where TData : class
{
    [JsonInclude]
    public string BusinessProcessDefinitionKey { get; } = businessProcessDefinitionKey;

    public StartVariables<TData> StartVariables { get; } = startVariables;
}