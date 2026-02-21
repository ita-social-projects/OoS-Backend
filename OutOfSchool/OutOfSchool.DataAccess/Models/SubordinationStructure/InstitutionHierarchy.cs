using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.SubordinationStructure;

public class InstitutionHierarchy : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    [MinLength(1)]
    [MaxLength(200)]
    public string Title { get; set; }

    public int HierarchyLevel { get; set; }

    public bool IsDeleted { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? RegistrySyncDate { get; set; }

    public Guid? ParentId { get; set; }

    public virtual InstitutionHierarchy Parent { get; set; }

    [Required]
    public Guid InstitutionId { get; set; }

    public virtual Institution Institution { get; set; }

    public virtual List<SubDirection> SubDirections { get; set; }

    #region Sports Registry Fields

    public long? SportRegistryIdCode { get; set; }

    [MaxLength(10)]
    public string? SportsSectionNumeral { get; set; }

    #endregion
}