using System.Text.Json.Serialization;

namespace OutOfSchool.AikomApiClient.Models;

public abstract class ApiRequest(string businessProcessDefinitionKey, StartVariables startVariables)
{
    [JsonInclude]
    public string BusinessProcessDefinitionKey { get; } = businessProcessDefinitionKey;

    public StartVariables StartVariables { get; } = startVariables;
}

public class StartVariables
{
    public required object Request { get; set; }
}
