using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class RegionAdmin : IKeyedEntity<(string, long)>
{
    [Key]
    public string UserId { get; set; }

    [Required(ErrorMessage = "InstitutionId is required")]
    public Guid InstitutionId { get; set; }

    public virtual Institution Institution { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [Required(ErrorMessage = "CATOTTGId is required")]
    public long CATOTTGId { get; set; }

    public virtual CATOTTG CATOTTG { get; set; }

    [NotMapped]
    public (string, long) Id
    {
        get => (UserId, CATOTTGId);
        set
        {
            UserId = value.Item1;
            CATOTTGId = value.Item2;
        }
    }
}