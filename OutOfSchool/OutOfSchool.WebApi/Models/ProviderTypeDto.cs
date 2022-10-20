using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models;

public class ProviderTypeDto
{
    public long Id { get; set; }

    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
}