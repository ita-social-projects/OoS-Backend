using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public abstract class InstitutionAdminBase : ISoftDeleted
{
    [Key]
    public string UserId { get; set; }

    public bool IsDeleted { get; set; }

    [Required(ErrorMessage = "InstitutionId is required")]
    public Guid InstitutionId { get; set; }

    public virtual Institution Institution { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}
