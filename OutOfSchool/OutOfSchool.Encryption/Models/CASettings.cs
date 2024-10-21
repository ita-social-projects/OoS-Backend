namespace OutOfSchool.Encryption.Models;

public class CASettings
{
    public string[] issuerCNs { get; set; }

    public string address { get; set; }

    public string ocspAccessPointAddress { get; set; }

    public string ocspAccessPointPort { get; set; }

    public string cmpAddress { get; set; }

    public string tspAddress { get; set; }

    public string tspAddressPort { get; set; }

    public bool directAccess { get; set; }
}