namespace OutOfSchool.SportsRegistryApiClient.Models;

static class RegistryConstants
{
    /// <summary>
    /// BPMN/Workflow process definition key used to create a section in the external registry.
    /// </summary>
    public const string SectionCreateProcessKey = "api-section-create";
    
    /// <summary>
    /// BPMN/Workflow process definition key used to update a section in the external registry.
    /// </summary>
    public const string SectionUpdateProcessKey = "api-section-update";

    /// <summary>Maximum allowed age in years for validation against the registry contract.</summary>
    public const int MaxAge = 120;
    
    public const string CreateAction = "creating";
    
    public const string UpdateAction = "updating";
    
    public const string DeleteAction = "deleting";
}

