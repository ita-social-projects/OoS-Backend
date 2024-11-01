using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Encryption.Config;

public class LDAP
{
    public bool Enabled { get; set; }

    [Required]
    public string Host { get; set; }

    [Range(0, 65535, ErrorMessage = "Port is out of range.")]
    public int Port { get; set; }

    public bool IsAnonymous { get; set; }

    public string User { get; set; }

    public string Password { get; set; }
}