using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class InstitutionAdmin : IKeyedEntity<(string, Guid)>
{
    [Key]
    public string UserId { get; set; }

    public Guid InstitutionId { get; set; }

    public virtual Institution Institution { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [NotMapped]
    public (string, Guid) Id
    {
        get => (UserId, InstitutionId);
        set
        {
            UserId = value.Item1;
            InstitutionId = value.Item2;
        }
    }
}