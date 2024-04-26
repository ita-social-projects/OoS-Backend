using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models;
public class PublicProvider : Provider
{
    [Required]
    public ProviderStatus Status { get; set; }

    [MaxLength(500)]
    public string StatusReason { get; set; }
}