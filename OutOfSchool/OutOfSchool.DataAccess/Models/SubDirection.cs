using OutOfSchool.Services.Models.SubordinationStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;
public class SubDirection : IKeyedEntity<long>, ISoftDeleted
{
    public long Id {  get; set; }

    public bool IsDeleted { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long DirectionId { get; set; }
    public virtual Direction Direction { get; set; }

    public virtual ICollection<InstitutionHierarchy> InstitutionHierarchies { get; set; }
}
