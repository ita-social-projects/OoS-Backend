using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderStatusDto
{
    [Required] public Guid ProviderId { get; set; }

    [Required]
    [EnumDataType(typeof(ProviderStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderStatus Status { get; set; }

    public string? StatusReason { get; set; } = default;
}

public static class ProviderStatusDtoExtensions
{
    public static ProviderStatusDto ToStatusDto(this Provider model)
        => new()
        {
            ProviderId = model.Id,
            Status = model.Status,
            StatusReason = string.Empty, // not model.StatusReason - see original AM mapping,
        };
}