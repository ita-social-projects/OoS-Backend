namespace OutOfSchool.WebApi.Models;

public class ProviderAdminSearchFilter : SearchStringFilter
{
    /// <summary>
    /// Gets or sets a value indicating whether to return only deputy provider admins.
    /// </summary>
    public bool DeputyOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to return only assistants (workshop) provider admins.
    /// </summary>
    public bool AssistantsOnly { get; set; }
}
