namespace OutOfSchool.IdentityServer.Config.ExternalUriModels;

/// <summary>
/// Contains external uris that are used with email operations.
/// </summary>
public class EmailScopeExternalUrisConfig
{
    public const string Name = "ExternalUris:EmailScope";

    /// <summary>
    /// Gets or sets login uri that can be used with email confirmation.
    /// </summary>
    public string EmailConfirmationRedirectToLogin { get; set; }
}