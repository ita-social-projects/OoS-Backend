using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthCommon.Config;

public class EUSignServicePaths
{
    [Required]
    public string Certificate { get; set; }

    [Required]
    public string Decrypt { get; set; }
}