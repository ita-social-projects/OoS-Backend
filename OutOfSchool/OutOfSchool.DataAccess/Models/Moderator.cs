using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models;

public class Moderator : ISoftDeleted, IKeyedEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; private set; }

    [DataType(DataType.DateTime)]
    public DateTime? UpdatedAt { get; private set; }

    [Column(TypeName = "char")]
    [MaxLength(36)]
    public string CreatedBy { get; private set; }

    [Column(TypeName = "char")]
    [MaxLength(36)]
    public string UpdatedBy { get; private set; }

    public virtual Individual Individual { get; set; }
}