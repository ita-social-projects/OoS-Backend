using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderStatusDto
{
    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    public ProviderStatus Status { get; set; }
}