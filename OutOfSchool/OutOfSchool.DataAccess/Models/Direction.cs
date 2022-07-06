using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class Direction : IKeyedEntity<long>
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public virtual List<Department> Departments { get; set; }

    public virtual ICollection<InstitutionHierarchy> InstitutionHierarchies { get; set; }
}