namespace OutOfSchool.IdentityServer.Config.ExternalUriModels;

public class EmailScopeExternalUrisConfig
{
    public const string Name = "ExternalUris:EmailScope";

    public string EmailConfirmationRedirectToLogin { get; set; }
}