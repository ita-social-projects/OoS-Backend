using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class Direction : IKeyedEntity<long>, ISoftDeleted
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual List<InstitutionHierarchy> InstitutionHierarchies { get; set; }
}