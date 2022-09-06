namespace OutOfSchool.WebApi.Common.Responses;

public sealed class ProviderApiError : ApiError
{
    public static readonly ProviderApiError ProviderNotCreated
        = new (nameof(ProviderNotCreated), "Provider was not created");

    public static readonly ProviderApiError ProviderNotUpdated
        = new (nameof(ProviderNotUpdated), "Provider was not updated");

    public static readonly ProviderApiError ProviderNotDeleted
        = new (nameof(ProviderNotDeleted), "Provider was not deleted");

    public static readonly ProviderApiError ProviderNotFound
        = new (nameof(ProviderNotFound), "Provider was not found");

    private ProviderApiError(string code, string message)
        : base(code, message)
    {
    }

    public override string GroupName => nameof(ProviderApiError);
}