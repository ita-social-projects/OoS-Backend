using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderBlockDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public bool IsBlocked { get; set; }

    [MaxLength(500)]
    public string BlockReason { get; set; }
}