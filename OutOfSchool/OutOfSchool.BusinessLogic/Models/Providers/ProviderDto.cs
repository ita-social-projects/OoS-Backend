using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderDto : ProviderBaseDto, IHasRating
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