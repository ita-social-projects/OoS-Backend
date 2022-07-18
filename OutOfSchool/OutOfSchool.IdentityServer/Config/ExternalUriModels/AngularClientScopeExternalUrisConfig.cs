namespace OutOfSchool.IdentityServer.Config.ExternalUriModels;

/// <summary>
/// Contains external uris that are used with OutOfSchoolAngular client.
/// </summary>
public class AngularClientScopeExternalUrisConfig
{
    public const string Name = "ExternalUris:AngularClientScope";

    /// <summary>
    /// Gets or sets login uri.
    /// </summary>
    public string Login { get; set; }
}