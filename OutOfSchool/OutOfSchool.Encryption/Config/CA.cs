using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Encryption.Config;

public class CA
{
    [Required]
    public string JsonPath { get; set; }

    [Required]
    public string CertificatesPath { get; set; }
}