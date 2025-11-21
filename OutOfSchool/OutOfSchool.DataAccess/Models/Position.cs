using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models;

public class Position : BusinessEntity
{
    [MaxLength(30)]
    public string Language { get; set; }

    [MaxLength(Constants.MaxPositionDescriptionLength)]
    public string Description { get; set; }

    public bool IsForRuralAreas { get; set; }

    [MaxLength(60)]
    public string Department { get; set; }

    public Guid ProviderId { get; set; }
    public virtual Provider Provider { get; set; }

    public Guid ContactId { get; set; }

    [Required(ErrorMessage = "SeatsAmount is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "SeatsAmount must be non-negative.")]
    public int SeatsAmount { get; set; }

    [Required(ErrorMessage = "FullName is required.")]
    [MaxLength(Constants.NameMaxLength)]
    public string FullName { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    public string ShortName { get; set; }

    [Required(ErrorMessage = "GenitiveName is required.")]
    [MaxLength(Constants.NameMaxLength)]
    public string GenitiveName { get; set; } = string.Empty;

    public bool IsTeachingPosition { get; set; }

    [Required(ErrorMessage = "Rate is required.")]
    public float Rate { get; set; }

    [Required(ErrorMessage = "Tariff is required.")]
    public float Tariff { get; set; }

    [Required(ErrorMessage = "ClassifierType is required.")]
    [MaxLength(60)]
    public string ClassifierType { get; set; } = string.Empty;
    
    [Required]
    public PositionType PositionType { get; set; } = PositionType.Employee;

    [Required(ErrorMessage = "IsPedagogicalPosition is required.")]
    public bool IsPedagogicalPosition { get; set; }

    [Required(ErrorMessage = "PositionOpenedByOrganization is required.")]
    public Guid PositionOpenedByOrganization { get; set; }

    [Required(ErrorMessage = "TotalRatesForPosition is required.")]
    public float TotalRatesForPosition { get; set; }

    [Required(ErrorMessage = "IsOffStaffPosition is required.")]
    public bool IsOffStaffPosition { get; set; }

    public virtual ICollection<Official> Officials { get; set; }
}