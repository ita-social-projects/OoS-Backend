using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.SubordinationStructure;

public class Institution : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    [MinLength(1)]
    [MaxLength(100)]
    public string Title { get; set; }

    [Range(1, int.MaxValue)]
    public int NumberOfHierarchyLevels { get; set; }

    public bool IsGovernment { get; set; }

    public bool IsDeleted { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    public virtual List<Provider> RelatedProviders { get; set; }
}