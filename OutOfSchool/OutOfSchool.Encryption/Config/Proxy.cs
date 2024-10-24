using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Encryption.Config;

public class Proxy
{
    public bool Enabled { get; set; }

    public string Host { get; set; }

    [Range(0, 65535, ErrorMessage = "Port is out of range.")]
    public string Port { get; set; }

    public string User { get; set; }

    public string Password { get; set; }
}