namespace OutOfSchool.AuthCommon.Config;

public class ExternalLogin
{
    public Uri IdServerUri { get; set; }

    public Uri EUSignServiceUri { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public Parameters Parameters { get; set; }

    public IdServerPaths IdServerPaths { get; set; }
}