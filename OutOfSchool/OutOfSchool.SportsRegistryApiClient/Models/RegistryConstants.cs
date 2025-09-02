namespace OutOfSchool.SportsRegistryApiClient.Models;

static class RegistryConstants
{
    /// <summary>
    /// BPMN/Workflow process definition key used to create a section in the external registry.
    /// </summary>
    public const string BusinessProcessDefinitionKey = "api-section-create";

    /// <summary>Maximum allowed age in years for validation against the registry contract.</summary>
    public const int MaxAge = 120;
}

