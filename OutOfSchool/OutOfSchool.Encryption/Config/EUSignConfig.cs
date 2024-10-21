namespace OutOfSchool.Encryption.Config;

public class EUSignConfig
{
    public static readonly string Name = "EUSign";

    public string DefaultOCSPServer { get; set; }

    public string DefaultTSPServer { get; set; }

    public PrivateKey PrivateKey { get; set; }

    public CA CA { get; set; }

    public Proxy Proxy { get; set; }
}