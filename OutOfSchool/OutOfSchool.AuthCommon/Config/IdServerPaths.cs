using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthCommon.Config;

public class IdServerPaths
{
    [Required]
    public string Authorize { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public string UserInfo { get; set; }
}