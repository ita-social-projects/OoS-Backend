using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Providers;

public class PublicProviderDto : ProviderDto
{
    [Required]
    [EnumDataType(typeof(ProviderStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderStatus Status { get; set; } = ProviderStatus.Pending;

    [MaxLength(500)]
    public string StatusReason { get; set; }
}
