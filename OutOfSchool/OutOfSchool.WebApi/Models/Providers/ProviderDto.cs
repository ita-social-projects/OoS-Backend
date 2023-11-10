using OutOfSchool.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderDto : ProviderBaseDto
{
    [Required]
    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; }

    public bool IsBlocked { get; set; }

    [MaxLength(500)]
    public string BlockReason { get; set; }

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    public string BlockPhoneNumber { get; set; } = string.Empty;
}