using OutOfSchool.Common.Config;

namespace OutOfSchool.IdentityServer.Config;

public class IdentityConnectionOptions : IMySqlGuidConnectionOptions
{
    public bool UseOverride { get; set; }

    public string Server { get; set; }

    public uint Port { get; set; }

    public string Database { get; set; }

    public string UserId { get; set; }

    public string Password { get; set; }

    public string GuidFormat { get; set; }
}