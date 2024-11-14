using System.Text.Json.Serialization;

namespace OutOfSchool.AikomApiClient.Models;

public class SearchUniversityRequest(string edrpou)
{
    [JsonInclude]
    public readonly string BusinessProcessDefinitionKey = "searchUniversity";

    public StartVariables StartVariables { get; } = new StartVariables(new Request(edrpou));
}

public class StartVariables(Request request)
{
    public Request Request { get; } = request;
}

public class Request(string edrpou)
{
    public string Edrpou { get; } = edrpou;
}