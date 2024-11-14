using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Encryption.Config;

public class EUSignConfig
{
    public static readonly string ConfigSectionName = "EUSign";

    [Required]
    public string DefaultOCSPServer { get; set; }

    [Range(0, 65535, ErrorMessage = "Port is out of range.")]
    public int DefaultOCSPPort { get; set; }

    [Required]
    public string DefaultTSPServer { get; set; }

    [Range(0, 65535, ErrorMessage = "Port is out of range.")]
    public int DefaultTSPPort { get; set; }

    public PrivateKey PrivateKey { get; set; }

    public CA CA { get; set; }

    public Proxy Proxy { get; set; }
    
    public LDAP LDAP { get; set; }
    
    public CMP CMP { get; set; }
    
    public FileStore FileStore { get; set; }
}