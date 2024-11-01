using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Encryption.Config;

public class CMP
{
    public bool Enabled { get; set; }

    [Required]
    public string Host { get; set; }

    [Range(0, 65535, ErrorMessage = "Port is out of range.")]
    public int Port { get; set; }

    public string CommonName { get; set; }
}