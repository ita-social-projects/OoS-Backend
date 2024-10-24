using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Encryption.Config;

public class EUSignConfig
{
    public static readonly string ConfigSectionName = "EUSign";

    public string DefaultOCSPServer { get; set; }

    [Range(0, 65535, ErrorMessage = "Port is out of range.")]
    public string DefaultOCSPPort { get; set; }

    public string DefaultTSPServer { get; set; }

    [Range(0, 65535, ErrorMessage = "Port is out of range.")]
    public string DefaultTSPPort { get; set; }

    public PrivateKey PrivateKey { get; set; }

    public CA CA { get; set; }

    public Proxy Proxy { get; set; }
}