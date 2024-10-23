namespace OutOfSchool.Encryption.Models;

public class CASettings
{
    public string[] IssuerCNs { get; set; }

    public string Address { get; set; }

    public string OcspAccessPointAddress { get; set; }

    public string OcspAccessPointPort { get; set; }

    public string CmpAddress { get; set; }

    public string TspAddress { get; set; }

    public string TspAddressPort { get; set; }

    public bool DirectAccess { get; set; }
}