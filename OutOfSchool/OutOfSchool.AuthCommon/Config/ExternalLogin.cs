using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthCommon.Config;

public class ExternalLogin
{
    [Required]
    public Uri IdServerUri { get; set; }

    [Required]
    public Uri EUSignServiceUri { get; set; }

    [Required]
    public string ClientId { get; set; }

    [Required]
    public string ClientSecret { get; set; }

    public Parameters Parameters { get; set; }

    public IdServerPaths IdServerPaths { get; set; }

    public EUSignServicePaths EUSignServicePaths { get; set; }
}