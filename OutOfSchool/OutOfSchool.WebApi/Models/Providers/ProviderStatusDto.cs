using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderStatusDto
{
    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    [EnumDataType(typeof(ProviderStatus), ErrorMessage = "Status should be in enum range")]
    public ProviderStatus Status { get; set; }
}