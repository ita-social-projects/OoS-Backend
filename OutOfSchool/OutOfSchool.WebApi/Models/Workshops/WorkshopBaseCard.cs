﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Workshops;

public class WorkshopBaseCard
{
    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    [MaxLength(120)]
    public string ProviderTitle { get; set; } = string.Empty;

    public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(1)]
    [MaxLength(60)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public PayRateType PayRate { get; set; }

    public string CoverImageId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    public bool CompetitiveSelection { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 10000, ErrorMessage = "Field value should be in a range from 1 to 10 000")]
    public decimal Price { get; set; } = default;

    public List<long> DirectionIds { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    public AddressDto Address { get; set; }

    public bool WithDisabilityOptions { get; set; }

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    [EnumDataType(typeof(ProviderLicenseStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderLicenseStatus ProviderLicenseStatus { get; set; } = ProviderLicenseStatus.NotProvided;
}
